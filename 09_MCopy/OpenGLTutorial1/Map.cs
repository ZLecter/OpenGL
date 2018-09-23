using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1 {

	class ExtraMath {
		public static int Map(int value, int startA, int stopA, int startB, int stopB) {
			value = (value - startA) / (stopA - startA);
			return startB + value * (stopB - startB);
		}

		public static double Map(double value, double startA, double stopA, double startB, double stopB) {
			value = (value - startA) / (stopA - startA);
			return startB + value * (stopB - startB);
		}

		public static float Map(float value, float startA, float stopA, float startB, float stopB) {
			value = (value - startA) / (stopA - startA);
			return startB + value * (stopB - startB);
		}

		public static int Vector2Distance(int x1, int y1, int x2, int y2) {
			//This is for corners
			if(y1==y2)
				return ((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2));
			else
				return ((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)) - 1;
		}
	}

	class Map {
		public Block[,,] map;
		int mapHeight = 16;
		int maxBlocks = 10;
		double probTreeSpawn = 0.1;

		//public static int RenderDistance = 3;

		public Map(int x, int y) {
			map = new Block[x, mapHeight,y];
			PerlinNoise noise = new PerlinNoise();
			Random rng = new Random();
			PerlinNoise.Seed = rng.Next(0, 123456789);
			PerlinNoise.Seed = 1;
			float perlinScale = (float)1 / 20;

			for(int i = 0; i < x; i++) {
				for(int j = 0; j < y; j++) {
					double yy = PerlinNoise.CalcPixel2D(i, j, perlinScale);
					yy = ExtraMath.Map(yy, 0, 255, 0, maxBlocks);
					int id = (int)yy;
					//Console.WriteLine(id);
					for(int k = id; k >= 0; k--) {

						Vector3 npos = new Vector3(i, k , j);
						int nid = GetIDByHeight(k);

						//We check if top block is GRASS
						if(nid == (int)TextureManager.BlockID.GRASS && k == id) {
							double prob = rng.NextDouble();
							if(prob < probTreeSpawn) {
								Console.WriteLine("WILL CREATE TREE at: " + i + ", " + j);
								GenerateTree(x, y, i, j, k, npos, rng);
							}
						}

						//Fill blocks below grass with dirt
						if(k < id && nid == (int)TextureManager.BlockID.GRASS)
							nid = (int)TextureManager.BlockID.DIRT;
						yy = PerlinNoise.CalcPixel2D(i * nid, j * nid, perlinScale);
						double xtra = ExtraMath.Map(yy, 0, 255, 0, 2);
						//Console.Write((int)xtra + " ");

						if((int)xtra == 0) {
							if(nid == (int)TextureManager.BlockID.SAND && k > 1)
								nid = (int)TextureManager.BlockID.DIRT;
						}

						if(rng.NextDouble() < 0.4f) {
							if(k == 2)
								nid = (int)TextureManager.BlockID.SAND;
							else if(k == 1)
								nid = (int)TextureManager.BlockID.STONE;
						}

						map[i, k, j] = new Block(nid, npos, Vector3.Zero, Vector3.One);
					}
				}
				//Console.WriteLine();
			}

		}


		public void GenerateTree(int x, int y, int i, int j, int k, Vector3 npos, Random rng) {
			int[,] tree = Tree.GenTree(rng, mapHeight-maxBlocks-1);
			int extraHeight = 1;
			//TI and TJ = position on the actual map
			//TX and TY = position of the tree
			for(int ti = i - 1, tx = 0; tx < tree.GetLength(0); ti++, tx++) {
				for(int tj = j - 1, ty = 0; ty < tree.GetLength(1); tj++, ty++) {

					//position valid on X axis and Y axis
					if((ti >= 0 && ti < x-1) && (tj >= 0 && tj < y-1)) {
						//There is no reason to build a block if its air/null
						if(tree[tx, ty] != 0) {
							
							Console.WriteLine("Tree created at " + ti + "-" + tj);
							Console.WriteLine("With tree tile at " + tx + "-" + ty);
							Console.WriteLine("Height:" + (k + extraHeight));
							Console.WriteLine("Block id: " + tree[tx, ty]);
							
							Vector3 tPos = new Vector3(ti, npos.Y + extraHeight, tj);

							Console.WriteLine("Vector WORLD: [" + (npos.X) + ", " + npos.Y + ", " + (npos.Z) + "]");
							Console.WriteLine("Vector MAP: [" + ti + ", " + (k + extraHeight) + ", " + tj + "]");
							//Dont replace existing blocks
							if(k+extraHeight < mapHeight) {
							if(map[ti, k + extraHeight, tj] == null)
								map[ti, k + extraHeight, tj] = new Block(tree[tx, ty], tPos, Vector3.Zero, Vector3.One);
							}
							Console.WriteLine();
						}
					}
					//Console.WriteLine();

				}
				//Si ti sobrepasa los 3 ciclos, resetearlo a i-1
				if((tx + 1) % 3 == 0) {
					//Console.WriteLine("Aqui se resetea TI a i-1\n");
					Console.WriteLine("-------------------");
					ti = i - 2;
					extraHeight++;
				}
			}
			Console.WriteLine("Tree has " + extraHeight + " of height");
		}

		private int GetIDByHeight(double h) {
			if(h < 1) {
				return (int)TextureManager.BlockID.STONE;
			} else if(h < 4){
				return (int)TextureManager.BlockID.SAND;
			} else if(h < 5){
				return (int)TextureManager.BlockID.DIRT;
			} else {
				return (int)TextureManager.BlockID.GRASS;
			}
		}

		public void Draw(ShaderProgram shader) {
			int blocksRendered = 0;

			//Should we render blocks based on camera's distance?
			
			//Optimize draws, only render block that has atleast one empty neighbour
			for(int y = 0; y < map.GetLength(1); y++){
				for(int x = 0; x < map.GetLength(0); x++){
					for(int z = 0; z < map.GetLength(2); z++){
						//No need to draw null block
						if(map[x, y, z] == null)
							continue;

						int pyDist = ExtraMath.Vector2Distance((int)-Game.player.position.X, -(int)Game.player.position.Z, x, z);
						if(pyDist > Game.player.RenderDistance * 2)
							continue;

						if(IsBlockOnEdge(x,y,z)){
							map[x, y, z].Draw(shader);
							blocksRendered++;
						} else if(IsBlockInside(x, y, z) && BlockCanBeDraw(x,y,z)) {
							map[x, y, z].Draw(shader);
							blocksRendered++;
						}

					}
				}
			}
			Console.WriteLine("Blocks Rendered Opt: " + blocksRendered);
			

			//RENDER ALL BLOCKS. THIS IS NOT OPTIMIZED
			/*
			blocksRendered = 0;
			foreach(Block b in map) {
				if(b != null){
					b.Draw(shader);
					blocksRendered++;
				}
			}
			Console.WriteLine("Blocks Rendered Normal: " + blocksRendered);
			*/

		}

		private bool BlockCanBeDraw(int x, int y, int z) {
			if(map[x, y, z + 1] == null || map[x, y, z - 1] == null ||
				map[x + 1, y, z] == null || map[x - 1, y, z] == null ||
				map[x, y + 1, z] == null) {
				return true;
			} else
				return false;
		}

		private bool IsBlockInside(int x, int y, int z) {
			if(x >= 1 && x < map.GetLength(0) - 1 &&
				y >= 1 && y < map.GetLength(1) - 1 &&
				z >= 1 && z < map.GetLength(2) - 1) {
				return true;
			} else
				return false;
		}

		private bool IsBlockOnEdge(int x, int y, int z) {
			if(z == 0 || x == 0 || y == 0 ||
				z == map.GetLength(1) - 1 || x == map.GetLength(0) - 1 || y == map.GetLength(2) - 1) {
				return true;
			} else
				return false;
		}

		public void Dispose() {
			foreach(Block b in map) {
				if(b != null)
					b.Dispose();
			}
		}
	}
}
