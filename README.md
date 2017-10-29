Thanks for checking out this repo!

It is still a work in progress, so I would love suggestions, comments, criticisms, etc.
Feel free to contact me here, or on twitter : @cabbibo

Right now, I only have the project working with 2 data sets : 

- tSNE Audio, compiled by Kyle McDonald, who
  - scraped audio from freesound.org : https://archive.org/details/freesound4s &
  - made t-SNE embeddings using : https://github.com/kylemcdonald/AudioNotebooks/

The scene that uses this data is: moveTest2 and is the main reason this project was created

 - Star Data from the HYG Database:
  - github here: https://github.com/astronexus/HYG-Database
  - more info here: http://www.astronexus.com/hyg
  
The scene that uses this data is: stars and is more or less a test to make sure the CSV to buffer actually works!


The main goal of this project was to see if I could visualize large data sets and also be able to select single objects from these complex data sets. This was done using Compute Buffers, and a concept called 'Parallel Reduction' to get that information back from the GPU to the CPU!

I feel like SO much more can be done with it so I wanted to open source the repo, but I understand that its pretty complex, so if you want to use the code please feel free to ask me questions!


The coolest files in here are the csvToBuffer which takes a csv file in, and gives you back a compute buffer of 3D positions using that information
You get to select which data is which dimension, but I do think that eventually creating procedural buffers that let you store arbitray lengths of data will be super helpful!

Anyways, get to creating, and please ask questions!
  
