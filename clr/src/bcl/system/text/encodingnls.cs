// ==++==
// 
//   
//    Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
//   
//    The use and distribution terms for this software are contained in the file
//    named license.txt, which can be found in the root of this distribution.
//    By using this software in any fashion, you are agreeing to be bound by the
//    terms of this license.
//   
//    You must not remove this notice, or any other, from this software.
//   
// 
// ==--==
namespace System.Text
{
    
    using System;
    using System.Collections;
    using System.Runtime.Remoting;
    using System.Globalization;
    using System.Threading;
    using Win32Native = Microsoft.Win32.Win32Native;
    
    // This class overrides Encoding with the things we need for our NLS Encodings
    //
    // All of the GetBytes/Chars GetByte/CharCount methods are just wrappers for the pointer
    // plus decoder/encoder method that is our real workhorse.  Note that this is an internal
    // class, so our public classes cannot derive from this class.  Because of this, all of the
    // GetBytes/Chars GetByte/CharCount wrapper methods are duplicated in all of our public
    // encodings, which currently include:
    //
    //      EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, & UnicodeEncoding
    //
    // So if you change the wrappers in this class, you must change the wrappers in the other classes
    // as well because they should have the same behavior.
    //
    [System.Runtime.InteropServices.ComVisible(true)]
    [Serializable()] internal abstract class EncodingNLS : Encoding
    {    
        protected EncodingNLS(int codePage) : base(codePage)
        {
        }

        // Returns the number of bytes required to encode a range of characters in
        // a character array.
        // 
        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        // parent method is safe
        public override unsafe int GetByteCount(char[] chars, int index, int count)
        {
            // Validate input parameters
            if (chars == null)
                throw new ArgumentNullException("chars", 
                      Environment.GetResourceString("ArgumentNull_Array"));

            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException((index<0 ? "index" : "count"), 
                      Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));

            if (chars.Length - index < count)
                throw new ArgumentOutOfRangeException("chars",
                      Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));

            // If no input, return 0, avoid fixed empty array problem
            if (chars.Length == 0)
                return 0;

            // Just call the pointer version
            fixed (char* pChars = chars)
                return GetByteCount(pChars + index, count, null);
        }

        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        // parent method is safe
        public override unsafe int GetByteCount(String s)
        {
            // Validate input
            if (s==null)
                throw new ArgumentNullException("s");

            fixed (char* pChars = s)
                return GetByteCount(pChars, s.Length, null);
        }       

        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        public override unsafe int GetByteCount(char* chars, int count)
        {
            // Validate Parameters
            if (chars == null)
                throw new ArgumentNullException("chars", 
                    Environment.GetResourceString("ArgumentNull_Array"));

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", 
                    Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            
            // Call it with empty encoder
            return GetByteCount(chars, count, null);
        }

        // Parent method is safe.
        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        public override unsafe int GetBytes(String s, int charIndex, int charCount,
                                              byte[] bytes, int byteIndex)
        {
            if (s == null || bytes == null)
                throw new ArgumentNullException((s == null ? "s" : "bytes"), 
                      Environment.GetResourceString("ArgumentNull_Array"));

            if (charIndex < 0 || charCount < 0)
                throw new ArgumentOutOfRangeException((charIndex<0 ? "charIndex" : "charCount"), 
                      Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));

            if (s.Length - charIndex < charCount)
                throw new ArgumentOutOfRangeException("s",
                      Environment.GetResourceString("ArgumentOutOfRange_IndexCount"));

            if (byteIndex < 0 || byteIndex > bytes.Length)
                throw new ArgumentOutOfRangeException("byteIndex", 
                    Environment.GetResourceString("ArgumentOutOfRange_Index"));

            int byteCount = bytes.Length - byteIndex;

            // Fixed doesn't like empty arrays
            if (bytes.Length == 0)
                bytes = new byte[1];

            fixed (char* pChars = s)
                fixed ( byte* pBytes = bytes)
                    return GetBytes(pChars + charIndex, charCount,
                                    pBytes + byteIndex, byteCount, null);
        }
    
        // Encodes a range of characters in a character array into a range of bytes
        // in a byte array. An exception occurs if the byte array is not large
        // enough to hold the complete encoding of the characters. The
        // GetByteCount method can be used to determine the exact number of
        // bytes that will be produced for a given range of characters.
        // Alternatively, the GetMaxByteCount method can be used to
        // determine the maximum number of bytes that will be produced for a given
        // number of characters, regardless of the actual character values.
        // 
        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        // parent method is safe
        public override unsafe int GetBytes(char[] chars, int charIndex, int charCount,
                                               byte[] bytes, int byteIndex)
        {
            // Validate parameters
            if (chars == null || bytes == null)
                throw new ArgumentNullException((chars == null ? "chars" : "bytes"), 
                      Environment.GetResourceString("ArgumentNull_Array"));

            if (charIndex < 0 || charCount < 0)
                throw new ArgumentOutOfRangeException((charIndex<0 ? "charIndex" : "charCount"), 
                      Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));

            if (chars.Length - charIndex < charCount)
                throw new ArgumentOutOfRangeException("chars",
                      Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));

            if (byteIndex < 0 || byteIndex > bytes.Length)
                throw new ArgumentOutOfRangeException("byteIndex",
                     Environment.GetResourceString("ArgumentOutOfRange_Index"));

            // If nothing to encode return 0, avoid fixed problem
            if (chars.Length == 0)
                return 0;

            // Just call pointer version
            int byteCount = bytes.Length - byteIndex;

            // Fixed doesn't like empty arrays
            if (bytes.Length == 0)
                bytes = new byte[1];

            fixed (char* pChars = chars)
                fixed (byte* pBytes = bytes)
                    // Remember that byteCount is # to decode, not size of array.
                    return GetBytes(pChars + charIndex, charCount,
                                    pBytes + byteIndex, byteCount, null);            
        }

        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        public override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount)
        {
            // Validate Parameters
            if (bytes == null || chars == null)
                throw new ArgumentNullException(bytes == null ? "bytes" : "chars",
                    Environment.GetResourceString("ArgumentNull_Array"));

            if (charCount < 0 || byteCount < 0)
                throw new ArgumentOutOfRangeException((charCount<0 ? "charCount" : "byteCount"), 
                    Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));

            return GetBytes(chars, charCount, bytes, byteCount, null);
        }                                              

        // Returns the number of characters produced by decoding a range of bytes
        // in a byte array.
        // 
        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        // parent method is safe
        public override unsafe int GetCharCount(byte[] bytes, int index, int count)
        {
            // Validate Parameters
            if (bytes == null)
                throw new ArgumentNullException("bytes", 
                    Environment.GetResourceString("ArgumentNull_Array"));

            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException((index<0 ? "index" : "count"), 
                    Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
                                   
            if (bytes.Length - index < count)
                throw new ArgumentOutOfRangeException("bytes",
                    Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));

            // If no input just return 0, fixed doesn't like 0 length arrays
            if (bytes.Length == 0)
                return 0;

            // Just call pointer version
            fixed (byte* pBytes = bytes)
                return GetCharCount(pBytes + index, count, null);
        }

        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        public override unsafe int GetCharCount(byte* bytes, int count)
        {
            // Validate Parameters
            if (bytes == null)
                throw new ArgumentNullException("bytes", 
                    Environment.GetResourceString("ArgumentNull_Array"));

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", 
                    Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            
            return GetCharCount(bytes, count, null);
        }        

        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        // parent method is safe
        public override unsafe int GetChars(byte[] bytes, int byteIndex, int byteCount,
                                              char[] chars, int charIndex)
        {
            // Validate Parameters
            if (bytes == null || chars == null)
                throw new ArgumentNullException(bytes == null ? "bytes" : "chars",
                    Environment.GetResourceString("ArgumentNull_Array"));

            if (byteIndex < 0 || byteCount < 0)
                throw new ArgumentOutOfRangeException((byteIndex<0 ? "byteIndex" : "byteCount"), 
                    Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));

            if ( bytes.Length - byteIndex < byteCount)
                throw new ArgumentOutOfRangeException("bytes",
                    Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));

            if (charIndex < 0 || charIndex > chars.Length)
                throw new ArgumentOutOfRangeException("charIndex", 
                    Environment.GetResourceString("ArgumentOutOfRange_Index"));

            // If no input, return 0 & avoid fixed problem
            if (bytes.Length == 0)
                return 0;

            // Just call pointer version
            int charCount = chars.Length - charIndex;

            // Fixed doesn't like empty arrays
            if (chars.Length == 0)
                chars = new char[1];

            fixed (byte* pBytes = bytes)
                fixed (char* pChars = chars)
                    // Remember that charCount is # to decode, not size of array
                    return GetChars(pBytes + byteIndex, byteCount,
                                    pChars + charIndex, charCount, null);
        }

        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount)
        {
            // Validate Parameters
            if (bytes == null || chars == null)
                throw new ArgumentNullException(bytes == null ? "bytes" : "chars",
                    Environment.GetResourceString("ArgumentNull_Array"));

            if (charCount < 0 || byteCount < 0)
                throw new ArgumentOutOfRangeException((charCount<0 ? "charCount" : "byteCount"), 
                    Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            
            return GetChars(bytes, byteCount, chars, charCount, null);
        }
    
        // Returns a string containing the decoded representation of a range of
        // bytes in a byte array.
        // 
        // All of our public Encodings that don't use EncodingNLS must have this (including EncodingNLS)
        // So if you fix this, fix the others.  Currently those include:
        // EncodingNLS, UTF7Encoding, UTF8Encoding, UTF32Encoding, ASCIIEncoding, UnicodeEncoding
        // parent method is safe
        public override unsafe String GetString(byte[] bytes, int index, int count)
        {
            // Validate Parameters
            if (bytes == null)
                throw new ArgumentNullException("bytes",
                    Environment.GetResourceString("ArgumentNull_Array"));

            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException((index < 0 ? "index" : "count"), 
                    Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));

            if (bytes.Length - index < count)
                throw new ArgumentOutOfRangeException("bytes",
                    Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));

            // Avoid problems with empty input buffer
            if (bytes.Length == 0) return String.Empty;
            
            fixed (byte* pBytes = bytes)
                return String.CreateStringFromEncoding(
                    pBytes + index, count, this);
        }

        public override Decoder GetDecoder()
        {
            return new DecoderNLS(this);
        }

        public override Encoder GetEncoder()
        {
            return new EncoderNLS(this);
        }
    }
}
