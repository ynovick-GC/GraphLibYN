import time

# displays how much time a graph took to create
def displayTime(graph_name, start, end):
    print("%.2f s \t%s" % (round(end - start,2), graph_name))

start = time.time()
import more_info_graphs
end = time.time()
displayTime("nodes_colored_by_value", start, end)

start = time.time()
import multi_graphs
end = time.time()
displayTime("multi_graphs", start, end)