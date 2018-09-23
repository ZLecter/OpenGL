using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1 {
	class Block {
		private int ID;
		private Texture texture;

		private VBO<Vector3> vertex;
		private VBO<int> elements;
		private VBO<Vector2> uvs;

		private Vector3 position;
		private Vector3 rotation;
		private Vector3 scale;

		public Block(int id) {
			this.position = Vector3.Zero;
			this.rotation = Vector3.Zero;
			this.scale = Vector3.One;
			ID = id;

			
			Init();
		}
		public Block(int id, Vector3 position, Vector3 rotation, Vector3 scale) {
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
			ID = id;

			Init();
		}

		private void Init() {
			texture = TextureManager.GetTextureByID(ID);
			// FACES in order: up, down, front, back, right, left
			vertex = new VBO<Vector3>(
				new Vector3[] {
					new Vector3(-0.5f,0.5f,-0.5f), new Vector3(0.5f,0.5f,-0.5f), new Vector3(0.5f,0.5f,0.5f), new Vector3(-0.5f,0.5f,0.5f),
					new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,0.5f), new Vector3(-0.5f,-0.5f,0.5f),
					new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,-0.5f), new Vector3(0.5f,0.5f,-0.5f), new Vector3(-0.5f,0.5f,-0.5f),
					new Vector3(-0.5f,-0.5f,0.5f), new Vector3(0.5f,-0.5f,0.5f), new Vector3(0.5f,0.5f,0.5f), new Vector3(-0.5f,0.5f,0.5f),
					new Vector3(0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,0.5f), new Vector3(0.5f,0.5f,0.5f), new Vector3(0.5f,0.5f,-0.5f),
					new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(-0.5f,-0.5f,0.5f), new Vector3(-0.5f,0.5f,0.5f), new Vector3(-0.5f,0.5f,-0.5f),
				}
			);

			elements = new VBO<int>(
				new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 },
				BufferTarget.ElementArrayBuffer
			);
			
			uvs = new VBO<Vector2>(TextureManager.GetUVByID(ID));
		}
		
		public void Draw(ShaderProgram program) {
			
			Gl.BindTexture(texture);
			program["model_matrix"].SetValue(
				Matrix4.CreateRotationX(rotation.X) *
				Matrix4.CreateRotationY(rotation.Y) *
				Matrix4.CreateRotationZ(rotation.Z) *
				Matrix4.CreateScaling(scale) *
				Matrix4.CreateTranslation(position));
			Gl.BindBufferToShaderAttribute(vertex, program, "vertexPosition");
			Gl.BindBufferToShaderAttribute(uvs, program, "vertexUV");
			Gl.BindBuffer(elements);
			Gl.DrawElements(BeginMode.Quads, elements.Count,
				DrawElementsType.UnsignedInt, IntPtr.Zero);

		}

		public void Dispose() {
			vertex.Dispose();
			elements.Dispose();
			uvs.Dispose();
		}

		public void Translate(Vector3 position) {
			this.position += position;
		}

		public void Rotate(Vector3 rotation) {
			this.rotation += rotation;
		}

		public void Scale(Vector3 scale) {
			this.scale += scale;
		}
	}
}
