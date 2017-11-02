import json
import networkx as nx
from networkx.readwrite import json_graph
import http_server

G = nx.Graph()
G.add_edges_from(
    [('A','B'), ('A','C'), ('B','C'), ('B','D'), ('C','E'), ('D','E'), ('D','F'), ('E','F')]
)
# this d3 example uses the name attribute for the mouse-hover value,
# so add a name to each node
for n in G:
    G.node[n]['name'] = n

# write json formatted data
d = json_graph.node_link_data(G) # node-link format to serialize

# write json
json.dump(d, open('force/force.json','w'))
print('Wrote node-link JSON data to force/force.json')

# open URL in running web browser
http_server.load_url('force/force.html')

print('Or copy all files in force/ to webserver and load force/force.html')