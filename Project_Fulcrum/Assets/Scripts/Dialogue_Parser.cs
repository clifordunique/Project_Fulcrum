using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.IO;  

public class Dialogue_Parser : MonoBehaviour {

	// Use this for initialization
	void Start () {
		//Load("Assets/Text/TEST1.txt");


	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private bool Load(string fileName)
	{
		// Handle any problems that might arise when reading the text
		try
		{
			string line;
			// Create a new StreamReader, tell it which file to read and what encoding the file
			// was saved as
			StreamReader reader = new StreamReader(fileName, Encoding.Default);
			// Immediately clean up the reader after this block of code is done.
			// You generally use the "using" statement for potentially memory-intensive objects
			// instead of relying on garbage collection.
			// (Do not confuse this with the using directive for namespace at the 
			// beginning of a class!)
			using (reader)
			{

				// While there's lines left in the text file, do this:
				do
				{
					line = reader.ReadLine();

					if (line != null)
					{
						// Do whatever you need to do with the text line, it's a string now
						// In this example, I split it into arguments based on comma
						// deliniators, then send that array to DoStuff()
						string[] components = line.Split(' ');
						foreach (string s in components)
						{
							print(s);
						}
					}
				}
				while (line != null);
				// Done reading, close the reader and return true to broadcast success    
				reader.Close();
				return true;
			}
		}
		// If anything broke in the try block, we throw an exception with information
		// on what didn't work
		catch (Exception e)
		{
			//print("FUCKKKKKKKKKK");
			Console.WriteLine("{0}\n", e.Message);
			return false;
		}
	}

}
