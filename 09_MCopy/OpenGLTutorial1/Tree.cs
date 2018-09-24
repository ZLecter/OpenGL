using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLTutorial1 {
	class Tree {
		//{{0,x,0},
		// {-z,0,+z}
		// {0,-x,0}}
		static int[,] b1 = {{0,0,0},
							{0,4,0},
							{0,0,0}};
		static int[,] b2 = {{5,5,5},
							{5,4,5},
							{5,5,5}};
		static int[,] b3 = {{0,5,0},
							{5,5,5},
							{0,5,0}};
		static int[,] b4 = {{0,0,0},
							{0,5,0},
							{0,0,0}};

		//Each layer is 3 height, so base1 equals the first 3 xs
		//Mininum height is 4 but 1,2 are constant. 1-2 for logs, 3 for leaves, and beyond are procedural
		public static int[,] GenTree(Random rng, int maxTreeHeight) {
			int extra = (int)rng.Next(1, maxTreeHeight);
			//Console.WriteLine("Extra tree height: " + extra);
			int tall = 3 + extra;
			//int tall = 3 + 2;
			//Console.WriteLine("TREE TALL: " + tall);
			int[,] tree = new int[tall*3,3];

			//Basic Tree
			#region
			//Log base 2 height
			tree[0, 0] = tree[3, 0] = b1[0, 0];
			tree[0, 1] = tree[3, 1] = b1[0, 1];
			tree[0, 2] = tree[3, 2] = b1[0, 2];

			tree[1, 0] = tree[4, 0] = b1[1, 0];
			tree[1, 1] = tree[4, 1] = b1[1, 1];
			tree[1, 2] = tree[4, 2] = b1[1, 2];

			tree[2, 0] = tree[5, 0] = b1[2, 0];
			tree[2, 1] = tree[5, 1] = b1[2, 1];
			tree[2, 2] = tree[5, 2] = b1[2, 2];
			#endregion

			//Starts at 3 height, with layer 1
			int prevLayer = 1;
			for(int i = 6; i < tall*3; i+=3) {
				int[,] currentLayer;

				//No more layers
				if(i+3 == tall*3 ) {
					//Console.WriteLine("Last layer");
					currentLayer = b4;
				} else {
					currentLayer = GetNextLayer(ref prevLayer, rng);
				}

				if(currentLayer != null) {
					tree[i + 0,	0] =	currentLayer[0, 0];
					tree[i + 0, 1] =	currentLayer[0, 1];
					tree[i + 0, 2] =	currentLayer[0, 2];

					tree[i + 1,	0] =	currentLayer[1, 0];
					tree[i + 1, 1] =	currentLayer[1, 1];
					tree[i + 1, 2] =	currentLayer[1, 2];

					tree[i + 2,	0] =	currentLayer[2, 0];
					tree[i + 2, 1] =	currentLayer[2, 1];
					tree[i + 2, 2] =	currentLayer[2, 2];

				}
			}
			
			/*
			Console.WriteLine("GENERATED TREE");
			for(int i = 0; i < tall*3; i++) {
				Console.Write(tree[i, 0] + " ");
				Console.Write(tree[i, 1] + " ");
				Console.Write(tree[i, 2] + " ");
				Console.WriteLine();

				if((i + 1) % 3 == 0)
					Console.WriteLine();

			}
			*/

			return tree;
		}

		//Return a layer based on the previous layer
		//b1 -> b2		b2 -> b1 || b3 || b4
		//b3 -> b4		b4 -> null
		static int[,] GetNextLayer(ref int prevLayer, Random rng) {
			double prob = rng.NextDouble();
			switch(prevLayer) {
				case 1:
				if(prob < 0.6) {
					prevLayer = 2;
					return b2;
				} else {
					return b1;
				}
				case 2:
				if(prob < 0.3) {
					prevLayer = 1;
					return b1;
				}else {
					prevLayer = 3;
					return b3;
				}
				case 3:
				prevLayer = 4;
				return b4;
				default:
				return b4;
			}
		}
	}
}
