﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSCI312_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("To compress a file enter Compress, to decompress a file enter Decompress\tTo exit type Exit");
            String enter = Console.ReadLine();
            String filePath = null;
            
            while(enter.ToLower() != "exit")
            {

                //****************Compression******************//

                if (enter.ToLower() == "compress")
                {
                    Console.WriteLine("Please enter in the absolute path for the file:");
                    filePath = Console.ReadLine();
                    try
                    {
                        LinkedList<CharacterFrequency> cflist = new LinkedList<CharacterFrequency>();
                        Console.WriteLine("Counting characters please wait!");
                        Charactercounting(filePath, cflist);
                        Console.WriteLine("Characters Counted!");
                        //Check to see if the file is empty
                        if (cflist.Count == 0)
                        {
                            Console.WriteLine("Cannot compress a file that is empty. Please re enter another command");
                            enter = Console.ReadLine();
                        }
                        else
                        {
                            //Variables created only if we're comrpessing and the file isnt empty.
                            String newFile = null;
                            String addName = "_Encode.txt";
                            LinkedList<encoding> encode = new LinkedList<encoding>();
                            LinkedList<BinaryTree<CharacterFrequency>> tree = new LinkedList<BinaryTree<CharacterFrequency>>();
                            //Write the cflist onto the encoding file here
                            newFile = getNewFileName(filePath, addName);
                            if(File.Exists(newFile))
                            {
                                File.Delete(newFile);
                            }

                            File.Create(newFile).Close();

                            StreamWriter sw = new StreamWriter(newFile,false);
                            
                            LinkedListNode<CharacterFrequency> node = cflist.First;
                            sw.WriteLine(cflist.Count + "\n");
                            while(node != null)
                            {
                                sw.WriteLine(node.Value.Print());
                                node = node.Next;
                            }
                            sw.Close();
                            String newfileText = getNewFileName(filePath, "_EncodeText.txt");
                            //Building tree
                            Console.WriteLine("Building tree, please wait");
                            buildTree(cflist, tree);
                            Console.WriteLine("Tree built!");
                            encodeTable(tree, encode);
                            //compression
                            Console.WriteLine("Compressing File please do NOT exit the program");
                            StreamReader sr = new StreamReader(filePath);
                            compress(encode, newfileText, sr);
                            sr.Close();
                            Console.WriteLine("File encoded!");
                            Console.WriteLine("Please enter another command");
                            enter = Console.ReadLine();
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("The file could not be opened, make sure you gave the right path to the file.");
                        Console.WriteLine("Please re enter another command");
                        enter = Console.ReadLine();
                    }
                    
                }

                    //************Decompression***************//

                else if (enter.ToLower() == "decompress")
                {
                    Console.WriteLine("Please enter the absolute path of the file that you want to decompress");
                    filePath = Console.ReadLine();
                    try
                    {
                        String encodeFile;
                        encodeFile = getNewFileName(filePath, "_Encode.txt");
                        String encodeText = getNewFileName(filePath, "_EncodeText.bin");
                        String decompressPath = getNewFileName(filePath, "_Decode.txt");
                        if (File.Exists(encodeFile)&& File.Exists(encodeText))
                        {
                            //Variables
                            StreamReader sr = new StreamReader(encodeFile);

                            LinkedList<CharacterFrequency> cflist = new LinkedList<CharacterFrequency>();
                            //get the total number of unique characters in the file so it knows when to stop;
                            int listcount = int.Parse(sr.ReadLine());
                            int counter = 0;
                            sr.ReadLine();
                            //gets the total number of characters in the file
                            //Creating the Tree
                            String line = sr.ReadLine();
                            Char[] split = new Char[] {','};
                            String[] results;
                            //Recreating the Linked List of CharacterFrequencys
                            while(counter < listcount)
                            {
                                results = line.Split(split, StringSplitOptions.None);
                                CharacterFrequency cf = new CharacterFrequency(int.Parse(results[0]), int.Parse(results[1]));
                                cflist.AddLast(cf);
                                sr.ReadLine();
                                line = sr.ReadLine();
                                counter++;
                            }
                            sr.Close();
                            //gets the total
                            int totalchar = total(cflist);
                            
                            LinkedList<BinaryTree<CharacterFrequency>> tree = new LinkedList<BinaryTree<CharacterFrequency>>();
                            Console.WriteLine("Building tree, please wait");
                            buildTree(cflist, tree);
                            Console.WriteLine("Tree built");
                            //Decompressing
                            Console.WriteLine("Decompressing File please do NOT exit the program");

                            decompress(tree, encodeText, decompressPath, totalchar);

                            Console.WriteLine("File Decompressed!");
                            Console.WriteLine("Please enter another command");
                            enter = Console.ReadLine();
                        }
                        else
                        {
                            Console.WriteLine("Cannot decompress a file when the encoding table could not be found");
                            Console.WriteLine("Either the encoding table was lost or the file you selected was not compressed");
                            Console.WriteLine("Please enter another command");
                            enter = Console.ReadLine();
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("The file could not be opened, make sure that you gave the right path to the file.");
                        Console.WriteLine("Please re enter another command");
                        enter = Console.ReadLine();
                    }
                }

                else
                {
                    Console.WriteLine("Please enter in a valid command");
                    enter = Console.ReadLine();
                }
            }
            Environment.Exit(0);
        }
        static LinkedList<CharacterFrequency> Charactercounting(String Filepath,LinkedList<CharacterFrequency> cflist)
        {
            StreamReader sr = new StreamReader(Filepath);
            int text = 0;
            text = sr.Read();
            if (text == -1)
            {
                sr.Close();
                return cflist;
            }
            else
            {
                //Reading the File
                while (text != -1)
                {
                    
                    CharacterFrequency cf = new CharacterFrequency((char)text, 1);
                    LinkedListNode<CharacterFrequency> node = cflist.Find(cf);

                    if (node != null)
                        node.Value.increment();
                    else
                        cflist.AddFirst(cf);

                    text = sr.Read();
                }
                sr.Close();
                //Ordering the Linked List
                CharacterFrequency[] cfa = cflist.ToArray();
                Array.Sort(cfa);

                cflist.Clear();

                for (int i=0; i < cfa.Length; i++)
                {
                    cflist.AddFirst(cfa[i]);
                }
                //Debugging
                //LinkedListNode<CharacterFrequency> snode = cflist.First;
                //while (snode != null)
                //{
                //    Console.WriteLine(snode.Value.ToString());
                //    snode = snode.Next;
                //}
                //End of debugging
                    return cflist;
            }
        }

        static LinkedList<BinaryTree<CharacterFrequency>> buildTree(LinkedList<CharacterFrequency> cflist, LinkedList<BinaryTree<CharacterFrequency>> tree)
        {
            LinkedListNode<CharacterFrequency> node = cflist.First;
            //Turn all the CharacterFrequency's into BinaryTree roots and add them to a LinkedList of Binarytrees of Characterfrequencys
            while (node != null)
            {
                BinaryTree<CharacterFrequency> bt = new BinaryTree<CharacterFrequency>();
                bt.Insert(node.Value, BinaryTree<CharacterFrequency>.Relative.root);
                tree.AddLast(bt);
                node = node.Next;
            }
            BinaryTree<CharacterFrequency> bt1 = new BinaryTree<CharacterFrequency>();
            BinaryTree<CharacterFrequency> bt2 = new BinaryTree<CharacterFrequency>();
            //Creating the Binary Tree
            while(tree.Count > 1)
            {
                //Remove first two elements
                bt1 = tree.First();
                tree.RemoveFirst();
                bt2 = tree.First();
                tree.RemoveFirst();
                //Sums the two elements and creates a new node
                int sum = bt1.Current.Data.count + bt2.Current.Data.count;
                CharacterFrequency cf = new CharacterFrequency('\0', sum);
                BinaryTree<CharacterFrequency> ptrBT_new = new BinaryTree<CharacterFrequency>();

                ptrBT_new.Insert(cf, BinaryTree<CharacterFrequency>.Relative.root);
                ptrBT_new.Insert(bt1.Root, BinaryTree<CharacterFrequency>.Relative.leftChild);
                ptrBT_new.Insert(bt2.Root, BinaryTree<CharacterFrequency>.Relative.rightChild);
                //Adds the Root into the list ordered by count from smallest to largest
                if (tree.Count == 0)
                    tree.AddFirst(ptrBT_new);
                else if (tree.First.Value.Root.Data.count >= ptrBT_new.Root.Data.count)
                    tree.AddFirst(ptrBT_new);
                else if (tree.Last.Value.Root.Data.count <= ptrBT_new.Root.Data.count)
                    tree.AddLast(ptrBT_new);
                else
                {
                    LinkedListNode<BinaryTree<CharacterFrequency>> treenode = tree.First;
                    while (treenode.Value.Root.Data.count < ptrBT_new.Root.Data.count && treenode != null)
                        treenode = treenode.Next;
                    tree.AddBefore(treenode, ptrBT_new);
                }
                //clean up
                cf = null;
                ptrBT_new = null;
                bt1 = null;
                bt2 = null;
            }        
                return tree;
        }
        static LinkedList<encoding> encodeTable(LinkedList<BinaryTree<CharacterFrequency>> tree, LinkedList<encoding> encodingTable)
        {
            tree.First.Value.Encode(tree.First.Value.Root, encodingTable);
            //Debugging
            //LinkedListNode<encoding> tableNode = encodingTable.First;
            //Console.WriteLine("Encoding Table:\n");
            //while(tableNode != null)
            //{
            //    Console.WriteLine(tableNode.Value.ToString());
            //    tableNode = tableNode.Next;
            //}
            //End of debugging
            return encodingTable;
        }

        static String getNewFileName(String orginialFileName, String addedName)
        {
            //Creates a new File with the same Path and name of the orginial file but adds in _Encode.txt at the end
            //of the file for storing the character Freq linked list to build the tree when decompressing
            //File Name without extension
            String NewFile = Path.GetFileNameWithoutExtension(orginialFileName);
            //the New File and its path
            String newFileName;
            //Gets File Name and extension
            String fileLength = Path.GetFileName(orginialFileName);
            
            newFileName = Path.GetFullPath(orginialFileName);
            //Gets the location to start removing from the new File Name
            int remove = newFileName.Length - fileLength.Length;
            //Removes the file name and the exention from the newfilename
            newFileName = newFileName.Remove(remove, fileLength.Length);
            //Gets the file name then adds in _Econde.txt for a new File Name
            NewFile = Path.GetFileNameWithoutExtension(orginialFileName);
            //Adds the extendedname to the orginial file name
            NewFile += addedName;
            //Adds the file Name to the path
            newFileName += NewFile;

            return newFileName;
        }

        static void compress(LinkedList<encoding> encoder, String filepath, StreamReader sr)
        {
            //checking to see if the file is there
            if(File.Exists(filepath))
            {
                File.Delete(filepath);
            }
            FileStream filestream = new FileStream(filepath, FileMode.OpenOrCreate,FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(filestream);
            //Variables
            byte b = 00000000;
            byte reset = 00000000;
            int read = sr.Read();
            int position = 7;
            encoding en = new encoding('\0', "\0");
            while(read != -1)
            {
                //searching for the encoding string
                en = new encoding((char)read, "\0");
                LinkedListNode<encoding> node = encoder.Find(en);
                String encode = node.Value.encode;     
                for(int i = 0; i < encode.Length; i++)
                {
                    if(encode[i] == '1')
                    {
                        //turning on bits
                        b = (byte)(b | (byte)(Math.Pow(2,position)));
                        position--;
                        //check to see if we're at the end of the byte
                        if(position < 0)
                        {
                            bw.Write(b);
                            b = (byte)(b & reset);
                            position = 7;
                        }
                    }
                    else
                    {
                        position--;
                        if(position < 0)
                        {
                            bw.Write(b);
                            b = (byte)(b & reset);
                            position = 7;
                        }
                    }
                }
                read = sr.Read();
            }
            bw.Write(b);

            bw.Close();
        }

        static void decompress(LinkedList<BinaryTree<CharacterFrequency>> tree, String compressPath,String decompressPath, int total)
        {
            FileStream filestream = new FileStream(compressPath, FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(filestream);

            if(!File.Exists(decompressPath))
            {
                File.Delete(decompressPath);
            }
            StreamWriter sw = new StreamWriter(decompressPath);

            BinaryTreeNode<CharacterFrequency> treeRoot = tree.First.Value.Root;
            BinaryTreeNode<CharacterFrequency> start = tree.First.Value.Root;     
            int count = 0;
            int index = 7;
            byte b = br.ReadByte();

            while(count < total)
            {
                //for(int i =7; i>=0; i--)
                while(index >=0)
                {
                    if(isbiton(b,index) == true)
                    {
                        treeRoot = treeRoot.Left;
                        index--;
                        if (treeRoot.isLeaf())
                        {
                            sw.Write(treeRoot.Data.character);
                            count++;
                            treeRoot = start;
                        }
                    }
                    else
                    {
                        treeRoot = treeRoot.Right;
                        index--;
                        if(treeRoot.isLeaf())
                        {
                            sw.Write(treeRoot.Data.character);
                            count++;
                            treeRoot = start;                            
                        }
                    }
                }
                if (index <= 0 && count < total)
                {
                    b = br.ReadByte();
                    index = 7;
                }
            }
            sw.Close();
        }
        public static int total(LinkedList<CharacterFrequency> cflist)
        {
            int total = 0;
            LinkedListNode<CharacterFrequency> node = cflist.First;
            while (node != null)
            {
                total += node.Value.count;
                node = node.Next;
            }
            return total;
        }

        public static bool isbiton(byte value, int index)
        {
            return (((byte)Math.Pow(2, index) & value) == (byte)Math.Pow(2,index));
        }
    }
}