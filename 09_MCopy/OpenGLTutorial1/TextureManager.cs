using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1 {
	class TextureManager {

		public enum BlockID {
			DIRT,//0
			GRASS,//1
			STONE,//2
			SAND,//3
			TREE_LOG,//4
			TREE_LEAVES,//5
			UNDEFINED//6
		}

		static Texture dirt = new Texture("Dirt.png");
		static Texture grass = new Texture("Grass.png");
		static Texture stone = new Texture("Stone.png");
		static Texture sand = new Texture("Sand.png");
		static Texture treeLog = new Texture("TreeLog.png");
		static Texture treeLeaves = new Texture("TreeLeaves.png");
		static Texture undefined = new Texture("Undefined.png");

		/**
		 * 
		 * @param id ID of the block (0=dirt, 1=grass, 2=stone, 3=sand, none=null)
		 */
		public static Texture GetTextureByID(int id) {
			Texture texture;
			switch(id) {
				case (int)BlockID.DIRT:
					texture = dirt;
					break;
				case (int)BlockID.GRASS:
					texture = grass;
					break;
				case (int)BlockID.STONE:
					texture = stone;
					break;
				case (int)BlockID.SAND:
					texture = sand;
					break;
				case (int)BlockID.TREE_LOG:
					texture = treeLog;
					break;
				case (int)BlockID.TREE_LEAVES:
					texture = treeLeaves;
					break;
				default:
					texture = undefined;
					break;
			}
			return texture;
		}

		public static Vector2[] GetUVByID(int id) {
			Vector2[] v;
			//Check if block needs to apply different textures to each face or only one
			if(id == (int)BlockID.GRASS || id ==(int)BlockID.TREE_LOG) {
				v = new Vector2[] {
					new Vector2(0.5f,0.5f), new Vector2(1,0.5f), new Vector2(1,1), new Vector2(0.5f,1),
					new Vector2(0,0), new Vector2(0.5f,0), new Vector2(0.5f,0.5f), new Vector2(0,0.5f),
					new Vector2(0,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,1), new Vector2(0,1),
					new Vector2(0,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,1), new Vector2(0,1),
					new Vector2(0,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,1), new Vector2(0,1),
					new Vector2(0,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,1), new Vector2(0,1)
				};
			} else {
				v = new Vector2[] {
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
				};
			}
			return v;
		}
	}
}
