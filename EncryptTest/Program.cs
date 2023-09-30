// See https://aka.ms/new-console-template for more information

using System.Net.Mime;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text;

String selectedOption;
Boolean appTurnedOn = true;
Byte[] nonce= new byte[32];
Byte[] ciphertext= new byte[32];
Byte[] tag = new byte[32];

string testKeyBecauseImLazy = "12345678912345678912345678912345";//test key.

Console.WriteLine("Welcome \n");

while (appTurnedOn)
{
    ShowOptions();
    ChooseOption();
}

void ShowOptions()
{
    Console.WriteLine("Choose one of the options below!");
    Console.WriteLine("1: Safely store message 2: Read message 0: Exit");
    selectedOption = Console.ReadLine();
}

void ChooseOption()
{
    if (selectedOption.Equals("1"))
    {
        Console.WriteLine("You selected Encrypt \n");
        EncryptOption();

    }else if (selectedOption.Equals("2"))
    {
        Console.WriteLine("You selected Decryption \n"); 
        DecryptOption();
    
    }else if (selectedOption.Equals("0"))
    {
        Console.WriteLine("exit selected :(");
        appTurnedOn = false;
    }
    else
    {
        Console.WriteLine("Did not get that. Please try again \n" );
    }
}

void EncryptOption()
{
    String plainText;
    String key;
    String fileName;
    
    Console.WriteLine("What do you want to call the file?");
    fileName = Console.ReadLine();
    
    Console.WriteLine("Please Give the text you want to secure");
    plainText = Console.ReadLine();
        
    Console.WriteLine("\n and please write a 32 character long key");
    key = Console.ReadLine();
    //key = testKeyBecauseImLazy;

    var keyByte = Encoding.ASCII.GetBytes(key);

    if (keyByte.Length == 32)
    {
        (ciphertext, nonce, tag) = EncryptWithNet(plainText, keyByte);
        WriteByteToFile(fileName, nonce, ciphertext, tag);
    }
    else
    {
        Console.WriteLine("that key is not real... not the right length \n");
    }
}

void DecryptOption()
{
    String key;
    String text;
    String fileName;
    
    Console.WriteLine("\nWhat is the file name ");
    fileName = Console.ReadLine();
    
    Console.WriteLine("\n plz give me the key");
    key = Console.ReadLine()!;
    //key = testKeyBecauseImLazy;
    
    var keyByte = Encoding.ASCII.GetBytes(key);

    if (keyByte.Length == 32)
    {
        ReadFromFile(fileName);//gets nonce, tag and keyByte from file.
        
        text = DecryptWithNet(ciphertext, nonce, tag, keyByte!);
        
        Console.WriteLine("\n this is your text:");
        Console.WriteLine(text);
        
        ciphertext = null;
        nonce = null;
        tag = null;
        keyByte = null;
    }else
    {
        Console.WriteLine("that key is not real... not the right length \n");
    }
}

static (byte[] ciphertext, byte[] nonce, byte[] tag) EncryptWithNet(string plaintext, byte[] key)
{
    using (var aes = new AesGcm(key))
    {
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];

        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        return (ciphertext, nonce, tag);
    }
}

static string DecryptWithNet(byte[] ciphertext, byte[] nonce, byte[] tag, byte[] key)
{
    using (var aes = new AesGcm(key))
    {
        try
        {
            var plaintextBytes = new byte[ciphertext.Length];
            aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);
            return Encoding.UTF8.GetString(plaintextBytes);
        }
        catch (Exception e)
        {
            return "\n Error: decryption failed, check password \n";
        }
    }
}


void WriteByteToFile(String fileName, byte[] nonce, byte[] ciphertext, byte[] tag)
{ 
    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + fileName + ".txt"))
    {
        Console.WriteLine("\n Error: This file already exist, please find a better name\n ");
    }else
    {
        // create file in root... i hope
        string pathName = AppDomain.CurrentDomain.BaseDirectory + fileName + ".txt";
        FileStream fileStream = File.Create(pathName);
        fileStream.Close();
        using (fileStream = new FileStream(pathName, FileMode.Create, FileAccess.Write, FileShare.None)) {
            using (BinaryWriter bw = new BinaryWriter(fileStream))
            {
                bw.Write(nonce.Length);
                bw.Write(nonce, 0, nonce.Length);
                bw.Write(ciphertext.Length);                    
                bw.Write(ciphertext, 0, ciphertext.Length);
                bw.Write(tag.Length);
                bw.Write(tag, 0, tag.Length);
            }      
            PrintEncryptInfo();
        }   
    }
}

void ReadFromFile(String fileName)
{
    if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + fileName + ".txt"))
    {
        Console.WriteLine("\nError: This file does not exist\n ");
    }else
    {
        using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + fileName + ".txt", FileMode.Open))
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                nonce = br.ReadBytes(br.ReadInt32());
                ciphertext = br.ReadBytes(br.ReadInt32());
                tag = br.ReadBytes(br.ReadInt32());
            }
        }
    }
}

//just some boring stuff for printing some info from here...
void PrintEncryptInfo()
{
    PrintByteArray("\n Cipher text: ",ciphertext);
    PrintByteArray("nonce text: ",nonce);
    PrintByteArray("tag text: ",tag);
    Console.WriteLine("");
}

void PrintByteArray(String text, Byte[] byteArray)
{
    Console.Write(text);
    for(int i=0; i< byteArray.Length ; i++) {
        Console.Write(byteArray[i] +" ");
    }
    Console.WriteLine("");
}
