﻿using System;
using Newtonsoft.Json;
using Siccity.GLTFUtility.Converters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#accessor
    /// <summary> Reads data from BufferViews </summary>
    public class GLTFAccessor : GLTFProperty {

#region Serialized fields
        public int? bufferView;
        public int byteOffset = 0;
        [JsonProperty(Required = Required.Always), JsonConverter(typeof(EnumConverter))] public AccessorType type;
        [JsonProperty(Required = Required.Always)] public GLType componentType;
        [JsonProperty(Required = Required.Always)] public int count;
        public float[] min;
        public float[] max;
        public Sparse sparse;
#endregion

        public class Cache {
            public byte[] bytes;
            public GLTFAccessor accessor;

            public Cache(byte[] bufferView) {
                bytes = bufferView;
                bytes = bufferView.SubArray(accessor.byteOffset, bufferView.Length - accessor.byteOffset);
            }
        }

        public Cache LoadCache(byte[][] bufferViews) {
            return new Cache(bufferViews[bufferView.Value]);
        }

        protected override bool OnLoad() {
            return true;
        }

        public Matrix4x4[] ReadMatrix4x4(byte[] bytes = null) {
            if (type != AccessorType.MAT4) {
                Debug.LogError("Type mismatch! Expected MAT4 got " + type);
                return new Matrix4x4[count];
            }

            Matrix4x4[] m = new Matrix4x4[count];
            if (bytes == null) bytes = glTFObject.bufferViews[bufferView.Value].GetBytes(byteOffset);
            int componentSize = GetComponentSize();
            Func<byte[], int, float> converter = GetFloatConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                m[i].m00 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m01 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m02 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m03 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m10 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m11 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m12 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m13 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m20 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m21 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m22 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m23 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m30 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m31 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m32 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m33 = converter(bytes, startIndex);
            }
            return m;
        }

        public Vector4[] ReadVec4(byte[] bytes = null) {
            if (type != AccessorType.VEC4) {
                Debug.LogError("Type mismatch! Expected VEC4 got " + type);
                return new Vector4[count];
            }

            Vector4[] verts = new Vector4[count];
            if (bytes == null) bytes = glTFObject.bufferViews[bufferView.Value].GetBytes(byteOffset);
            int componentSize = GetComponentSize();
            Func<byte[], int, float> converter = GetFloatConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                verts[i].x = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].y = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].z = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].w = converter(bytes, startIndex);
            }
            return verts;
        }

        public Color[] ReadColor(byte[] bytes = null) {
            if (type != AccessorType.VEC4 && type != AccessorType.VEC3) {
                Debug.LogError("Type mismatch! Expected VEC4 or VEC3 got " + type);
                return new Color[count];
            }

            Color[] colors = new Color[count];
            if (bytes == null) bytes = glTFObject.bufferViews[bufferView.Value].GetBytes(byteOffset);
            int componentSize = GetComponentSize();
            if (componentType == GLType.BYTE || componentType == GLType.UNSIGNED_BYTE) {
                Color32 color = Color.black;
                for (int i = 0; i < count; i++) {
                    int startIndex = i * componentSize;
                    color.r = bytes[startIndex];
                    startIndex += GetComponentTypeSize(componentType);
                    color.g = bytes[startIndex];
                    startIndex += GetComponentTypeSize(componentType);
                    color.b = bytes[startIndex];
                    if (type == AccessorType.VEC4) {
                        startIndex += GetComponentTypeSize(componentType);
                        color.a = bytes[startIndex];
                    } else {
                        color.a = (byte) 255;
                    }
                    colors[i] = color;
                }
            } else if (componentType == GLType.FLOAT) {
                Func<byte[], int, float> converter = GetFloatConverter();
                for (int i = 0; i < count; i++) {
                    int startIndex = i * componentSize;
                    colors[i].r = converter(bytes, startIndex);
                    startIndex += GetComponentTypeSize(componentType);
                    colors[i].g = converter(bytes, startIndex);
                    startIndex += GetComponentTypeSize(componentType);
                    colors[i].b = converter(bytes, startIndex);
                    if (type == AccessorType.VEC4) {
                        startIndex += GetComponentTypeSize(componentType);
                        colors[i].a = converter(bytes, startIndex);
                    } else {
                        colors[i].a = 1;
                    }
                }
            } else {
                Debug.LogWarning("Unexpected componentType! " + componentType);
            }

            return colors;
        }

        public Vector3[] ReadVec3(byte[] bytes = null) {
            if (type != AccessorType.VEC3) {
                Debug.LogError("Type mismatch! Expected VEC3 got " + type);
                return new Vector3[count];
            }
            if (bufferView == -1) {
                Debug.LogError("Accessor bufferView was unassigned");
                return new Vector3[count];
            }

            Vector3[] verts = new Vector3[count];
            if (bytes == null) bytes = glTFObject.bufferViews[bufferView.Value].GetBytes(byteOffset);
            int componentSize = GetComponentSize();
            Func<byte[], int, float> converter = GetFloatConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                verts[i].x = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].y = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].z = converter(bytes, startIndex);
            }
            return verts;
        }

        public Vector2[] ReadVec2(byte[] bytes = null) {
            if (type != AccessorType.VEC2) {
                Debug.LogError("Type mismatch! Expected VEC2 got " + type);
                return new Vector2[count];
            }
            if (componentType != GLType.FLOAT) {
                Debug.LogError("Non-float componentType not supported. Got " + (int) componentType);
                return new Vector2[count];
            }

            Vector2[] verts = new Vector2[count];
            if (bytes == null) bytes = glTFObject.bufferViews[bufferView.Value].GetBytes(byteOffset);
            int componentSize = GetComponentSize();
            Func<byte[], int, float> converter = GetFloatConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                verts[i].x = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].y = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
            }
            return verts;
        }

        public float[] ReadFloat(byte[] bytes = null) {
            if (type != AccessorType.SCALAR) {
                Debug.LogError("Type mismatch! Expected SCALAR got " + type);
                return new float[count];
            }

            float[] floats = new float[count];
            if (bytes == null) bytes = glTFObject.bufferViews[bufferView.Value].GetBytes(byteOffset);
            int componentSize = GetComponentSize();
            Func<byte[], int, float> converter = GetFloatConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                floats[i] = converter(bytes, startIndex);
            }
            return floats;
        }

        public int[] ReadInt(byte[] bytes = null) {
            if (type != AccessorType.SCALAR) {
                Debug.LogError("Type mismatch! Expected SCALAR got " + type);
                return new int[count];
            }

            int[] ints = new int[count];
            if (bytes == null) bytes = glTFObject.bufferViews[bufferView.Value].GetBytes(byteOffset);
            int componentSize = GetComponentSize();
            Func<byte[], int, int> converter = GetIntConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                ints[i] = converter(bytes, startIndex);
            }
            return ints;
        }

        public Func<byte[], int, float> GetFloatConverter() {
            switch (componentType) {
                case GLType.BYTE:
                    return (x, y) => (float) (sbyte) x[y];
                case GLType.UNSIGNED_BYTE:
                    return (x, y) => (float) x[y];
                case GLType.FLOAT:
                    return System.BitConverter.ToSingle;
                case GLType.SHORT:
                    return (x, y) => (float) System.BitConverter.ToInt16(x, y);
                case GLType.UNSIGNED_SHORT:
                    return (x, y) => (float) System.BitConverter.ToUInt16(x, y);
                case GLType.UNSIGNED_INT:
                    return (x, y) => (float) System.BitConverter.ToUInt16(x, y);
                default:
                    Debug.LogWarning("No componentType defined");
                    return System.BitConverter.ToSingle;
            }
        }

        public Func<byte[], int, int> GetIntConverter() {
            switch (componentType) {
                case GLType.BYTE:
                    return (x, y) => (int) (sbyte) x[y];
                case GLType.UNSIGNED_BYTE:
                    return (x, y) => (int) x[y];
                case GLType.FLOAT:
                    return (x, y) => (int) System.BitConverter.ToSingle(x, y);
                case GLType.SHORT:
                    return (x, y) => (int) System.BitConverter.ToInt16(x, y);
                case GLType.UNSIGNED_SHORT:
                    return (x, y) => (int) System.BitConverter.ToUInt16(x, y);
                case GLType.UNSIGNED_INT:
                    return (x, y) => (int) System.BitConverter.ToUInt16(x, y);
                default:
                    Debug.LogWarning("No componentType defined");
                    return (x, y) => (int) System.BitConverter.ToUInt16(x, y);
            }
        }

        /// <summary> Get the size of the attribute type, in bytes </summary>
        public int GetComponentSize() {
            return GetComponentNumber(type) * GetComponentTypeSize(componentType);
        }

        public static int GetComponentTypeSize(GLType componentType) {
            switch (componentType) {
                case GLType.BYTE:
                    return 1;
                case GLType.UNSIGNED_BYTE:
                    return 1;
                case GLType.SHORT:
                    return 2;
                case GLType.UNSIGNED_SHORT:
                    return 2;
                case GLType.FLOAT:
                    return 4;
                case GLType.UNSIGNED_INT:
                    return 4;
                default:
                    Debug.LogError("componentType " + (int) componentType + " not supported!");
                    return 0;
            }
        }

        public static int GetComponentNumber(AccessorType type) {
            switch (type) {
                case AccessorType.SCALAR:
                    return 1;
                case AccessorType.VEC2:
                    return 2;
                case AccessorType.VEC3:
                    return 3;
                case AccessorType.VEC4:
                    return 4;
                case AccessorType.MAT2:
                    return 4;
                case AccessorType.MAT3:
                    return 9;
                case AccessorType.MAT4:
                    return 16;
                default:
                    Debug.LogError("type " + type + " not supported!");
                    return 0;
            }
        }

        // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#sparse
        public class Sparse {
            [JsonProperty(Required = Required.Always)] public int count;
            [JsonProperty(Required = Required.Always)] public Indices indices;
            [JsonProperty(Required = Required.Always)] public Values values;

            // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#values
            public class Values {
                [JsonProperty(Required = Required.Always)] public int bufferView;
                public int byteOffset = 0;

            }

            // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#indices
            public class Indices {
                [JsonProperty(Required = Required.Always)] public int bufferView;
                [JsonProperty(Required = Required.Always)] public int componentType;
                public int byteOffset = 0;
            }
        }
    }
}