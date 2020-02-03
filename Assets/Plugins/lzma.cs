using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public class lzma {

#if (UNITY_ANDROID|| UNITY_STANDALONE_LINUX || UNITY_BLACKBERRY) && !UNITY_EDITOR
		[DllImport("lzma", EntryPoint = "decompress7zip")]
		internal static extern int decompress7zip(string filePath, string exctractionPath, bool fullPaths,  string entry, ref int progress);
		[DllImport("lzma", EntryPoint = "decompress7zip2")]
		internal static extern int decompress7zip2(string filePath, string exctractionPath, bool fullPaths, string entry, ref int progress);
		[DllImport("lzma", EntryPoint = "_getSize")]
		internal static extern int _getSize(string filePath, string tempPath);
		[DllImport("lzma", EntryPoint = "lzmaUtil")]
		internal static extern int lzmaUtil(bool encode, string inPath, string outPath);
		[DllImport("lzma", EntryPoint = "decode2Buf")]
		internal static extern int decode2Buf(string filePath, string entry,  IntPtr buffer);
		[DllImport("lzma", EntryPoint = "_releaseBuffer")]
		internal static extern void _releaseBuffer(IntPtr buffer);
		[DllImport("lzma", EntryPoint = "Lzma_Compress")]
		internal static extern IntPtr Lzma_Compress( IntPtr buffer, int bufferLength, bool makeHeader, ref int v);
		[DllImport("lzma", EntryPoint = "Lzma_Uncompress")]
		internal static extern int Lzma_Uncompress( IntPtr buffer, int bufferLength, int uncompressedSize,  IntPtr outbuffer,bool useHeader);
#endif

#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
	    public static extern int getProgressCount();
		[DllImport("__Internal")]
	    private static extern int decompress7zip(string filePath, string exctractionPath, bool fullPaths,  string entry, ref int progress);
		[DllImport("__Internal")]
		private static extern int decompress7zip2(string filePath, string exctractionPath, bool fullPaths, string entry, ref int progress);
		[DllImport("__Internal")]
		private static extern int _getSize(string filePath, string tempPath);
		[DllImport("__Internal")]
		internal static extern int lzmaUtil(bool encode, string inPath, string outPath);
		[DllImport("__Internal")]
		internal static extern int decode2Buf(string filePath, string entry,  IntPtr buffer);
		[DllImport("__Internal")]
		internal static extern void _releaseBuffer(IntPtr buffer);	
		[DllImport("__Internal")]
		internal static extern IntPtr Lzma_Compress( IntPtr buffer, int bufferLength, bool makeHeader, ref int v);
		[DllImport("__Internal")]
		internal static extern int Lzma_Uncompress( IntPtr buffer, int bufferLength, int uncompressedSize,  IntPtr outbuffer,bool useHeader);
#endif

#if  UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
                      [DllImport("liblzma", EntryPoint = "decompress7zip")]
		internal static extern int decompress7zip(string filePath, string exctractionPath, bool fullPaths,  string entry, ref int progress);
		[DllImport("liblzma", EntryPoint = "decompress7zip2")]
        internal static extern int decompress7zip2(string filePath, string exctractionPath, bool fullPaths, string entry, ref int progress);
		[DllImport("liblzma", EntryPoint = "_getSize")]
		internal static extern int _getSize(string filePath, string tempPath);
		[DllImport("liblzma", EntryPoint = "lzmaUtil")]
		internal static extern int lzmaUtil(bool encode, string inPath, string outPath);
		[DllImport("liblzma", EntryPoint = "decode2Buf")]
		internal static extern int decode2Buf(string filePath, string entry,  IntPtr buffer);
		[DllImport("liblzma", EntryPoint = "_releaseBuffer")]
		internal static extern void _releaseBuffer(IntPtr buffer);
		[DllImport("liblzma", EntryPoint = "Lzma_Compress")]
		internal static extern IntPtr Lzma_Compress( IntPtr buffer, int bufferLength, bool makeHeader, ref int v);
		[DllImport("liblzma", EntryPoint = "Lzma_Uncompress")]
		internal static extern int Lzma_Uncompress( IntPtr buffer, int bufferLength, int uncompressedSize, IntPtr outbuffer,bool useHeader);
#endif


#if (UNITY_METRO || UNITY_WP_8_1 || UNITY_WSA)  && !UNITY_EDITOR
		[DllImport("liblzma", EntryPoint = "decompress7zip2")]
		internal static extern int decompress7zip2(string filePath, string exctractionPath, bool fullPaths, string entry, ref int progress);
		[DllImport("liblzma", EntryPoint = "_getSize")]
		internal static extern int _getSize(string filePath, string tempPath);
		[DllImport("liblzma", EntryPoint = "decode2Buf")]
		internal static extern int decode2Buf(string filePath, string entry,  IntPtr buffer);
#endif

    // An integer variable to store the total number of files in a 7z archive, excluding the folders.
    public static int trueTotalFiles = 0;

    //ERROR CODES:
    //  1 : OK
    //	2 : Could not find requested file in archive
    // -1 : Could not open input(7z) file
    // -2 : Decoder doesn't support this archive
    // -3 : Can not allocate memory
    // -4 : CRC error of 7z file
    // -5 : Unknown error
    // -6 : Can not open output file (usually when the path to write to, is invalid)
    // -7 : Can not write output file
    // -8 : Can not close output file

    //The most common use of this library is to download a 7z file in your Application.persistentDataPath directory
    //and decompress it in a folder that you want.

    //int lz=lzma.doDecompress7zip(Application.persistentDataPath+"/myCompresedFile.7z",Application.persistentDataPath+"/myUncompressedFiles/");

	//filePath			: the full path to the archive, including the archives name. (/myPath/myArchive.7z)
	//exctractionPath	: the path in where you want your files to be extracted
    //progress          : a referenced integer to get the progress of the extracted files (use this function when calling from a separate thread, otherwise call the 2nd implementation)
    //                  : (for ios this integer is not properly updated. So we use the lzma.getProgressCount() function to get the progress. See example.)
	//largeFiles		: set this to true if you are extracting files larger then 30-40 Mb. It is slower though but prevents crashing your app when extracting large files!
	//fullPaths			: set this to true if you want to keep the folder structure of the 7z file.
	//entry				: set the name of a single file file you want to extract from your archive. If the file resides in a folder, the full path should be added.
	//					   (for examle  game/meshes/small/table.mesh )
    //use this function from a separate thread to get the preogress  of the extracted files in the referenced 'progress' integer.
    //
	public static int doDecompress7zip(string filePath, string exctractionPath, ref int progress, bool largeFiles=false, bool fullPaths=false, string entry=null){

		//make a check if the last '/' exists at the end of the exctractionPath and add it if it is missing
		if( exctractionPath.Substring(exctractionPath.Length-1,1) != "/") { exctractionPath+="/"; }

		#if !UNITY_WSA || UNITY_EDITOR		
			if(largeFiles){
                return decompress7zip(filePath, exctractionPath, fullPaths, entry, ref progress);
			}else{
                return decompress7zip2(filePath, exctractionPath, fullPaths, entry, ref progress);
			}
		#endif

		#if (UNITY_METRO || UNITY_WP_8_1 || UNITY_WSA) && !UNITY_EDITOR
			return decompress7zip2(filePath, exctractionPath, fullPaths, entry, ref progress);
        #endif
    }

    //same as above only the progress integer is a local variable.
    //use this when you don't want to get the progress of the extracted files and when not calling the function from a separate thread.
    public static int doDecompress7zip(string filePath, string exctractionPath,  bool largeFiles = false, bool fullPaths = false, string entry = null)
    {

        //make a check if the last '/' exists at the end of the exctractionPath and add it if it is missing
        if (exctractionPath.Substring(exctractionPath.Length - 1, 1) != "/") { exctractionPath += "/"; }

        int progress = 0;

        #if !UNITY_WSA || UNITY_EDITOR
            if (largeFiles)
            {
                return decompress7zip(filePath, exctractionPath, fullPaths, entry, ref progress);
            }
            else
            {
                return decompress7zip2(filePath, exctractionPath, fullPaths, entry, ref progress);
            }
        #endif

        #if (UNITY_METRO || UNITY_WP_8_1 || UNITY_WSA) && !UNITY_EDITOR
			return decompress7zip2(filePath, exctractionPath, fullPaths, entry, ref progress);
        #endif
    }

#if !(UNITY_WSA || UNITY_WP_8_1 || UNITY_METRO)  || UNITY_EDITOR

    //ERROR CODES (for both encode/decode LzmaUtil functions):
	//   1 : OK
	// -10 : Can not read input file
	// -11 : Can not write output file
	// -12 : Can not allocate memory
	// -13 : Data error
	
	//This functions encodes a single archive in lzma alone format.
	//inPath	: the file to be encoded. (use full path + file name)
	//outPath	: the .lzma file that will be produced. (use full path + file name)
	public static int LzmaUtilEncode(string inPath, string outPath){
		return lzmaUtil(true, inPath, outPath);
	}


	//This function decodes a single archive in lzma alone format.
	//inPath	: the .lzma file that will be decoded. (use full path + file name)
	//outPath	: the decoded file. (use full path + file name)
	public static int LzmaUtilDecode(string inPath, string outPath){
		return lzmaUtil(false, inPath, outPath);
	}
#endif

	//Lists get filled with filenames (including path if the file is in a folder) and uncompressed file sizes
	public static List <string> ninfo = new List<string>();//filenames
	public static List <long> sinfo = new List<long>();//file sizes

	//this function fills the ArrayLists with the filenames and file sizes that are in the 7zip file
	//returns			: the total size in bytes of the files in the 7z archive 
	//
	//filePath			: the full path to the archive, including the archives name. (/myPath/myArchive.7z)
	//tempPath			: (optional) a temp path that will be used to write the files info (otherwise the path of the 7z archive will be used)
	//					: this is useful when your 7z archive resides in a read only location.
    //
    //trueTotalFiles is an integer variable to store the total number of files in a 7z archive, excluding the folders.
	public static long get7zInfo(string filePath, string tempPath=null){

		ninfo.Clear(); sinfo.Clear();
        trueTotalFiles = 0;

		if(tempPath==null){
			if(File.Exists(filePath+".txt")) File.Delete(filePath+".txt");
		}else{
			if(File.Exists(tempPath+".txt")) File.Delete(tempPath+".txt");	
		}

		int res = _getSize(filePath, tempPath);

		if(res==-1) { Debug.Log("Input file not found."); return -1; }
		
		string logPath;
		if(tempPath==null) logPath=filePath; else logPath=tempPath;
		logPath+=".txt";

		if(!File.Exists(logPath)) { Debug.Log("Info file not found."); return -3; }

		StreamReader r = new StreamReader(logPath);

		string line;
		string[] rtt;
		long t=0, sum=0;

		while ((line = r.ReadLine()) != null){
			rtt = line.Split('|');
			ninfo.Add(rtt[0]);
			long.TryParse(rtt[1],out t);
			sum += t;
			sinfo.Add(t);
            if (t > 0) trueTotalFiles++;
		}

		r.Close(); r.Dispose();
		File.Delete(logPath);

		return sum;
	}
	
	//this function returns the uncompressed file size of a given file in the 7z archive if specified,
	//otherwise it will return the total uncompressed size of all the files in the archive.
	//
	//If you don't fill the filePath parameter it will assume that the get7zInfo function has already been called.
	//
	//
	//filePath			: the full path to the archive, including the archives name. (/myPath/myArchive.7z)
	// 					: if you call the function with filePath as null, it will try to find file sizes from the last call.
	//fileName 			: the file name we want to get the file size (if it resides in a folder add the folder path also)
	//tempPath			: (optional) a temp path that will be used to write the files info (otherwise the path of the 7z archive will be used)
	//					: this is useful when your 7z archive resides in a read only location.
	public static long get7zSize( string filePath=null, string fileName=null, string tempPath=null){
		
		if(filePath!=null){
			if(get7zInfo(filePath, tempPath) < 0){Debug.Log(1); return -1;}
		}

		if(ninfo == null){
			if(ninfo.Count==0) {Debug.Log(2); return -1; }
		}

		long sum=0;

		if(fileName!=null){
			for(int i=0; i<ninfo.Count; i++){
				if(ninfo[i].ToString() == fileName){
					return (long)sinfo[i];
				}
			}
		}else{
			for(int i=0; i<ninfo.Count; i++){
				sum += (long)sinfo[i];
			}
			return sum;
		}
		return -1;//nothing was found
	}


	//A function to decode a specific archive in a 7z archive to a byte buffer
	//
	//filePath		: the full path to the 7z archive 
	//entry			: the file name to decode to a buffer. If the file resides in a folder, the full path should be used.
	//tempPath		: (optional) a temp path that will be used to write the files info (otherwise the path of the 7z archive will be used)
	//					: this is useful when your 7z archive resides in a read only location.
	public static byte[] decode2Buffer(  string filePath, string entry, string tempPath=null){
		
		int bufs = (int)get7zSize( filePath, entry, tempPath );

        if (bufs <= 0) return null;//entry error or it does not exist

        byte[] nb = new byte[bufs];

        GCHandle dec2buf = GCHandle.Alloc(nb, GCHandleType.Pinned);

		int res = decode2Buf(filePath, entry, dec2buf.AddrOfPinnedObject());

        dec2buf.Free();
		if(res==1){ return nb;}
		else {nb=null; return null; }

    }

#if !(UNITY_WSA || UNITY_WP_8_1 || UNITY_METRO) || UNITY_EDITOR

    //This function encodes inBuffer to lzma alone format into the outBuffer provided.
	//The buffer can be saved also into a file and can be opened by applications that open the lzma alone format.
	//This buffer can be uncompressed by the decompressBuffer function.
	//Returns true if success
    //if makeHeader==false then the lzma 13 bytes header will not be added to the buffer.
	public static  bool compressBuffer(byte[] inBuffer, ref byte[] outBuffer, bool makeHeader=true){

		GCHandle cbuf = GCHandle.Alloc(inBuffer, GCHandleType.Pinned);
		IntPtr ptr;
        
        int res = 0;

		ptr = Lzma_Compress(cbuf.AddrOfPinnedObject(), inBuffer.Length, makeHeader, ref res);

		cbuf.Free(); 

		if(res==0 || ptr==IntPtr.Zero){_releaseBuffer(ptr); return false;}

		System.Array.Resize(ref outBuffer,res);
		Marshal.Copy(ptr, outBuffer, 0, res);

		_releaseBuffer(ptr);

		return true;

	}



    //This function decompresses an lzma compressed byte buffer.
    //If the useHeader flag is false you have to provide the uncompressed size of the buffer via the customLength integer.
    //if res==0 operation was succesful
    //The error codes
    /*
        OK 0
		
        ERROR_DATA 1
        ERROR_MEM 2
        ERROR_UNSUPPORTED 4
        ERROR_PARAM 5
        ERROR_INPUT_EOF 6
        ERROR_OUTPUT_EOF 7
        ERROR_FAIL 11
        ERROR_THREAD 12
        */
	public static  int decompressBuffer(byte[] inBuffer,  ref byte[] outbuffer, bool useHeader=true, int customLength=0){
		
		GCHandle cbuf = GCHandle.Alloc(inBuffer, GCHandleType.Pinned);
		int uncompressedSize = 0;
		
		//if the lzma header will be used to extract the uncompressed size of the buffer. If the buffer does not have a header 
		//provide the known uncompressed size throught the customLength integer.
		if(useHeader) uncompressedSize = (int)BitConverter.ToUInt64(inBuffer,5); else uncompressedSize = customLength;

		//byte[] outbuffer = new byte[uncompressedSize];
		System.Array.Resize(ref outbuffer, uncompressedSize);

		GCHandle obuf = GCHandle.Alloc(outbuffer, GCHandleType.Pinned);
		
		int res = Lzma_Uncompress(cbuf.AddrOfPinnedObject(), inBuffer.Length, uncompressedSize, obuf.AddrOfPinnedObject(), useHeader);

		cbuf.Free();
		obuf.Free();

		if(res!=0){Debug.Log("ERROR: "+res.ToString()); }
	
		return res;		
	}

#endif

}

