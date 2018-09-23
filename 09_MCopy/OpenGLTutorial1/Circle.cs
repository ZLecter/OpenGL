using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1 {
	class Circle {
		private float radius;
		private int faces;
		private Texture texture;

		private VBO<Vector3> vertex;
		private VBO<int> elements;
		private VBO<Vector2> uvs;

		private Vector3 position;
		private Vector3 rotation;
		private Vector3 scale;

		public Circle() {
			this.radius = 1;
			this.faces = 24;
			position = Vector3.Zero;
			rotation = Vector3.Zero;
			scale = Vector3.One;
			Init();
		}

		public Circle(float radius, int faces) {
			this.radius = radius;
			this.faces = faces;
			position = Vector3.Zero;
			rotation = Vector3.Zero;
			scale = Vector3.One;
			Init();
		}

		public Circle(float radius, int faces, Vector3 position, Vector3 rotation, Vector3 scale) {
			this.radius = radius;
			this.faces = faces;
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
			Init();
		}

		private void Init() {
			texture = new Texture("crate.jpg");

			float anglePerFace = (float)360 / faces;
			Vector3[] v = new Vector3[faces * 3];
			Vector3 prevVert = new Vector3(radius, 0, 0);
			for(int i = 0, currentFace = 1; i < faces * 3; i += 3, currentFace++) {
				float angle = currentFace * anglePerFace;
				float x = (float)Math.Cos(angle * (Math.PI / 180)) * radius;
				float y = (float)Math.Sin(angle * (Math.PI / 180)) * radius;
				v[i] = new Vector3(0, 0, 0);
				v[i + 1] = prevVert;
				v[i + 2] = new Vector3(x, y, 0);
				prevVert = v[i + 2];
			}

			int[] ce = new int[faces * 3];
			for(int i = 0; i < faces * 3; i++) {
				ce[i] = i;
			}

			Vector2[] uv = new Vector2[faces * 4];
			for(int i = 0; i < faces * 4; i += 4) {
				uv[i] = new Vector2(0, 0);
				uv[i + 1] = new Vector2(1, 0);
				uv[i + 2] = new Vector2(1, 1);
				uv[i + 3] = new Vector2(0, 1);
			}

			elements = new VBO<int>(ce, BufferTarget.ElementArrayBuffer);
			vertex = new VBO<Vector3>(v);
			uvs = new VBO<Vector2>(uv);
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
			Gl.DrawElements(BeginMode.Triangles, elements.Count,
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
