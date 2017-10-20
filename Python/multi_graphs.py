import networkx as nx
import matplotlib.pyplot as plt
import decimal

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
            s = (sum(neighbors_degree) / len(neighbors_degree)) / degree
        result[node] = s

    return result

def floatToString(inputValue):
    return ('%.15f' % inputValue).rstrip('0').rstrip('.')

graphs = {}

# create regular graph
graphs["Regular Graph"] = G = nx.Graph()
# G.add_edges_from(
#     [('A','B'), ('A','C'),('D','B'),('E','C'),('E','F'),
#      ('B', 'H'),('B','G'),('B','F'),('C','G')]
# )
G.add_edges_from(
    [('A','B'),
     ('B','A'),('B','C'),('B','G'),
     ('C','B'),('C','D'),
     ('D','C'),('D','E'),
     ('E', 'D'), ('E', 'F'), ('E','H'),
     ('F', 'E'),
     ('G', 'B'), ('G', 'H'),
     ('H', 'G'),('H','E')]
)

# create complete graph
graphs["Complete Graph"] = \
    nx.complete_graph(20) # 100 nodes

# create random ER graph
ra_probability = 0.1
graphs["Erdos-Renyi Graph, p=" + str(ra_probability)] = \
    G_random_er = nx.erdos_renyi_graph(20, ra_probability) # (n, p)

# create random BA graph
ba_probability = 18
graphs["Barabasi-Albert Graph, p=" + str(ba_probability)] = \
    G_random_ba = nx.barabasi_albert_graph(20, ba_probability)

# plot graphs
fig = plt.figure(figsize=(15, 15))
index = 1
for g in graphs:
    i_num = index + 555
    chart = fig.add_subplot(i_num)

    # style
    chart.set_title(g)
    chart.set_axis_off()

    graph = graphs[g]

    # F-Index
    fiVector = getFiVector(graph) # returns dict { node : avg_neighbor_degree }
    for i in fiVector:
        fiVector[i] = floatToString(round(fiVector[i], 2))

    # plot graph
    if index == 1 or index == 4:
        pos = nx.spring_layout(graph)
    else:
        pos = nx.random_layout(graph)
    nx.draw_networkx_nodes(graph, pos, node_size=300)
    nx.draw_networkx_edges(graph, pos)
    nx.draw_networkx_labels(graph, pos, fiVector, font_size=8)

    index = index + 1

# show graph
plt.subplots_adjust(wspace=0)
plt.margins(0,0)
plt.tight_layout()
plt.savefig('graphs/multi_graphs.png', bbox_inches='tight', pad_inches=0.2)