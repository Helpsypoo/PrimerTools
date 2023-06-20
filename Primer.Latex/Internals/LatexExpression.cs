using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    public class LatexExpression : IReadOnlyList<LatexChar>, ISerializable
    {
        private readonly LatexChar[] characters;
        private readonly LatexInput input;

        public Vector2 center => GetBounds().center;
        public string source => input.code;

        public int Count => characters.Length;
        public LatexChar this[int i] => characters[i];


        public LatexExpression(LatexInput input)
        {
            this.input = input;
            characters = Array.Empty<LatexChar>();
        }

        public LatexExpression(LatexInput input, LatexChar[] chars)
        {
            this.input = input;
            characters = chars;
        }


        public bool IsSame(LatexExpression other)
        {
            return !ReferenceEquals(null, other) && characters.SequenceEqual(other.characters);
        }

        public Rect GetBounds()
        {
            var allVertices = new Vector2[characters.Length * 2];

            for (var i = 0; i < characters.Length; i++) {
                var charBounds = characters[i].bounds;
                allVertices[i * 2] = charBounds.min;
                allVertices[i * 2 + 1] = charBounds.max;
            }

            return VectorUtils.Bounds(allVertices);
        }

        public LatexExpression Slice(int start, int end)
        {
            var modifiedInput = input with { code = "{input.code} |slice({start},{end})" };
            var chars = characters.Take(end).Skip(start).ToArray();
            return new LatexExpression(modifiedInput, chars);
        }


        public override string ToString()
        {
            return $"LatexExpression({input?.code ?? "null"})[{characters.Length}]";
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(input), input);
            info.AddValue(nameof(characters), characters);
        }

        public IEnumerator<LatexChar> GetEnumerator() => characters.ToList().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
