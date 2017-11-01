using System;
using System.Text;
using System.Collections.Generic;

using Nutdeep.Utils.Extensions;

namespace Nutdeep.Tools
{
    public abstract class MemoryHelper
    {
        private Dictionary<Type,object> ParserDictionary 
            = new Dictionary<Type, object>()
        {
            {typeof(short),new ShortParser()},
            {typeof(int),new IntParser()},
            {typeof(long),new LongParser()},
            {typeof(ushort),new UShortParser()},
            {typeof(uint),new UIntParser()},
            {typeof(ulong),new ULongParser()}   ,
            {typeof(bool), new BooleanParser()},
            {typeof(char), new CharParser()},
            {typeof(sbyte), new SignedByteParser()},
            {typeof(byte), new ByteParser()},
            {typeof(float), new FloatParser()},
            {typeof(double), new DoubleParser()},
            {typeof(decimal), new DecimalParser()},
            {typeof(string), new StringParser()}
        };

        protected T Parse<T>(byte[] array)
            => ((ParsingHelper<T>) ParserDictionary[typeof(T)]).Parse(array);

        protected byte[] Parse<T>(T obj)
            => ((ParsingHelper<T>) ParserDictionary[typeof(T)]).Parse(obj);
        
        private abstract class ParsingHelper<T>
        {
            public abstract T Parse(byte[] array);

            public abstract byte[] Parse(T obj);
        }
        
        private class ShortParser : ParsingHelper<short>
        {
            public override short Parse(byte[] array)
                => BitConverter.ToInt16(array,0);

            public override byte[] Parse(short obj)
                => BitConverter.GetBytes(obj);
        }
        
        private class UShortParser : ParsingHelper<ushort>
        {
            public override ushort Parse(byte[] array)
                => BitConverter.ToUInt16(array, 0);

            public override byte[] Parse(ushort obj)
                => BitConverter.GetBytes(obj);
        }
        
        private class IntParser : ParsingHelper<int>
        {
            public override int Parse(byte[] array)
                => BitConverter.ToInt32(array, 0);

            public override byte[] Parse(int obj)
                => BitConverter.GetBytes(obj);
            
        }
        
        private class UIntParser : ParsingHelper<uint>
        {
            public override uint Parse(byte[] array)
                => BitConverter.ToUInt32(array, 0);

            public override byte[] Parse(uint obj)
                => BitConverter.GetBytes(obj);
        }
        
        private class LongParser : ParsingHelper<long>
        {
            public override long Parse(byte[] array)
                => BitConverter.ToInt64(array, 0);

            public override byte[] Parse(long obj)
                => BitConverter.GetBytes(obj);
        }
        
        private class ULongParser : ParsingHelper<ulong>
        {
            public override ulong Parse(byte[] array)
                => BitConverter.ToUInt64(array, 0);

            public override byte[] Parse(ulong obj)
                => BitConverter.GetBytes(obj);
        }
        
        private class BooleanParser : ParsingHelper<bool>
        {
            public override bool Parse(byte[] array)
                => BitConverter.ToBoolean(array, 0);

            public override byte[] Parse(bool obj)
                => BitConverter.GetBytes(obj);
        }
        
        private class CharParser : ParsingHelper<char>
        {
            public override char Parse(byte[] array)
                => BitConverter.ToChar(array, 0);

            public override byte[] Parse(char obj)
                => BitConverter.GetBytes(obj);
        }
        
        private class SignedByteParser : ParsingHelper<sbyte>
        {
            public override sbyte Parse(byte[] array)
                => Convert.ToSByte(array);

            public override byte[] Parse(sbyte obj)
                => new[] {Convert.ToByte(obj)};
        }
        
        private class ByteParser : ParsingHelper<byte>
        {
            public override byte Parse(byte[] array)
                => array[0];

            public override byte[] Parse(byte obj)
                => new[] {obj};
        }
        
        private class FloatParser : ParsingHelper<float>
        {
            public override float Parse(byte[] array)
                => BitConverter.ToSingle(array,0);

            public override byte[] Parse(float obj)
                => BitConverter.GetBytes(obj);
        }
        
        private class DoubleParser : ParsingHelper<double>
        {
            public override double Parse(byte[] array)
                => BitConverter.ToDouble(array,0);

            public override byte[] Parse(double obj)
                => BitConverter.GetBytes(obj);
        }
        
        private class DecimalParser : ParsingHelper<decimal>
        {
            public override decimal Parse(byte[] array)
                => array.ToDecimal();

            public override byte[] Parse(decimal obj)
                => obj.ToByteArray();
        }
        
        private class StringParser : ParsingHelper<string>
        {
            public override string Parse(byte[] array)
                => Encoding.ASCII.GetString(array);

            public override byte[] Parse(string obj)
                => Encoding.ASCII.GetBytes(obj);
        }
    }
}