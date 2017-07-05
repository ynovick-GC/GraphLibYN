# GraphLibYN
A C# class library for analyzing simple graphs

I'm trying to take time this summer to create a small class library that I'll be able to use for analyzing simple graphs. I must have created this on the fly a million times this year but I want to put some thought into it now and make something that I'll be able to reuse for different research projects that will come up.

The graphs themselves will be simple. Unweighted and undirected. My particular needs right now are for studies relating to degree so the functionality in these classes gives degree vector, FI vector (something I'm working on) and the like. If I need to extend it at some point hopefully that won't be too difficult.

Presently, the main feature is that it calculates all vectors using deferred exectuion, but remembers them for the next query. But, if the graph changes, the vectors are reset. If the classes are extended it's important to make sure this functionality is consistent, if other vectors are added they should follow the same logic.

The classes also generate Erdos Renyi and Barabasi Albert random graphs. These graphs do not have unit testing but I ran them a number of times against results from the networkx python package and I was satisfied that they are working.

There is a lot of unit testing on the graph class itself. There's room for improvement, but a lot of important cases are covered.

There are many other important details I'm sure, but this is a nice summary of the highlights.

-YN 7/4/17
