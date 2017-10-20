import networkx as nx
import matplotlib.pyplot as plt
import utils
import matplotlib.gridspec as gridspec
import statistics
import operator

def getFiVector(graph):
    result = {} # dict in form { node:neighbors' average neighbors}, so { 'A':4, 'B':0.3, ... }

    for node in graph:
        degree = graph.degree(node)
        neighbors_degree = []
        for neighbor in graph.neighbors(node):
            neighbors_degree.append(graph.degree(neighbor))
        if (degree == 0):
            s = 0
        else:
            s = (float(sum(neighbors_degree)) / len(neighbors_degree)) / float(degree)
        result[node] = s

    return result

def floatToString(inputValue):
    return ('%.15f' % inputValue).rstrip('0').rstrip('.')

graphs = {}

# create regular graph
graphs["Regular Graph"] = G = nx.Graph()
G.add_edges_from(
    [('A','B'), ('A','C'), ('B','C'), ('B','D'), ('C','E'), ('D','E'), ('D','F'), ('E','F')]
)

# create complete graph
graphs["Complete Graph"] = \
    nx.complete_graph(20) # 20 nodes

# create random ER graph
ra_probability = 0.1
graphs["Erdos-Renyi Graph\np=" + str(ra_probability)] = \
    G_random_er = nx.erdos_renyi_graph(20, ra_probability) # (n, p)

# create random BA graph
ba_probability = 18
graphs["Barabasi-Albert Graph\np=" + str(ba_probability)] = \
    G_random_ba = nx.barabasi_albert_graph(20, ba_probability)

# plot graphs
fig = plt.figure(figsize=(6, 12))
gs = gridspec.GridSpec(4, 2)

index = 1
for g in graphs:
    chart = fig.add_subplot(gs[index-1, -1])
    text_pane = fig.add_subplot(gs[index-1, -2])

    graph = graphs[g]

    # f-Index
    fiVector = getFiVector(graph) # returns dict { node : avg_neighbor_degree }
    for i in fiVector:
        fiVector[i] = floatToString(round(fiVector[i], 2))

    # plot graph
    pos = nx.circular_layout(graph)

    # place letters near nodes
    posX = [pos[p][0] for p in pos]
    posY = [pos[p][1] for p in pos]

    minX = min(posX)
    minY = min(posY)
    maxX = max(posX)
    maxY = max(posY)

    for p in pos:
        x,y = pos[p]
        if (x > utils.mean([minX, maxX])):
            loc_x = x + .1
        else:
            loc_x = x - .15

        if (y > utils.mean([minY, maxY])):
            loc_y = y + .1
        else:
            loc_y = y - .15

        plt.text(loc_x, loc_y, s=p, color='lightcoral', size=7, horizontalalignment='center')

    text_pane.set_title(g, fontsize=11)
    chart.axis('off')

    # statistics
    statsLabel = ""
    statsLabel += "Degree Sequence: " + str(sorted(nx.degree(graph).values(),reverse=True)) + "\n"
    statsLabel += "FI Vector: " + str(sorted(fiVector.items(), key=operator.itemgetter(1))) + "\n"
    statsLabel += "Average FI: " + str(round(utils.mean(list([float(x) for x in fiVector.values()])), 3)) + "\n"
    statsLabel += "Standard Deviation: " + str(round(statistics.stdev([float(x) for x in fiVector.values()]), 3)) + "\n"

    # print, for convenience
    print(g)
    print(statsLabel)

    # draw nodes based on fiVector value
    overshadowed_nodes = [node for node in graph if float(fiVector[node]) > 1]
    undershadowed_nodes = [node for node in graph if float(fiVector[node]) < 1]
    nodes_value_one = [node for node in graph if float(fiVector[node]) == 1]

    # colors can be found at http://matplotlib.org/mpl_examples/color/named_colors.hires.png
    node_size = 200
    nx.draw_networkx_nodes(graph, pos, nodelist=overshadowed_nodes, node_color='mediumturquoise', node_size=node_size)
    nx.draw_networkx_nodes(graph, pos, nodelist=undershadowed_nodes, node_color='gainsboro',node_size=node_size)
    nx.draw_networkx_nodes(graph, pos, nodelist=nodes_value_one, node_color='lightblue',node_size=node_size)

    # remove axis ticks and labels
    text_pane.tick_params(axis='both', which='both', bottom='off', top='off', left='off', right='off', labelleft='off',
                    labelbottom='off')
    chart.tick_params(axis='both', which='both', bottom='off', top='off', left='off', right='off', labelleft='off',
                    labelbottom='off')

    nx.draw_networkx_edges(graph, pos)
    nx.draw_networkx_labels(graph, pos, fiVector, font_size=7)

    text_pane.text(0.05, 0.98, statsLabel, ha='left', va="top", wrap=True,
         fontsize=7, transform=chart.transAxes)
    plt.margins(0.15, 0.15)

    index = index + 1

# show graph
plt.subplots_adjust(wspace=0)
plt.margins(0,0)
plt.tight_layout()
plt.savefig('graphs/more_info_graphs.png', bbox_inches='tight', pad_inches=0.1)