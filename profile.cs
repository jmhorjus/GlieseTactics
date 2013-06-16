using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class Profile
{

    public string userName;
    public int Win;
    public int Loss;
    public int Credit;
}

public class Test
{

    public static void Main()
    {

        //Read and write profiles
        Test t = new Test();
        t.CreateProfile("profile.xml");
        t.ReadProfile("profile");
    }

    private void CreateProfile(string filename)
    {

        //Creates an instance of the XmlSerializer class;
        //Specifies the type of object to serialize.
        XmlSerializer serializer = new XmlSerializer(typeof(Profile));
        TextWriter writer = new StreamWRiter(filename);
        Profile p = new Profile();

        //populates data
        p.userName = "OptimusPrime";
        p.Win = 0;
        p.Loss = 0;
        p.Credits = 0;

        //Serializes the profile and closes TextWriter
        serializer.Serialize(writer, p);
        writer.Close();
    }

    protected void ReadProfile(string filename)
    {

        //Creates an instance of the XmlSerializer class;
        //Specifies the type of object to be deserialized.
        XmlSerializer serializer = new XmlSerializer(typeof(Profile));

        //If the XML document has been altered with unkown
        //nodes or attributes, handle them with the
        //UnknownNode and UnknownAttribute events.
        serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
        serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

        //A FileStream is needed to read the XML document.
        FileStream fs = new FileStream(filename, FileMode.Open);

        //Declares an object variable of the type to be deserialized.
        Profile p;

        //Uses the deserialize method to restore the object's state
        //with data from the XML document
        p = (Profile)serializer.Deserialize(fs);

        Console.WriteLine("User Name: " + p.userName);
        Console.WriteLine("\nWins: " + p.Win);
        Console.WriteLine("\nLoss: " + p.Loss);
        Console.WriteLine("\nCredits: " + p.Credit);
    }

    protected void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
    {

        Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
    }

    protected void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
    {

        System.Xml.XmlAttribute attr = e.Attr;
        Console.WriteLine("Unknown attribute " +
        attr.Name + "='" + attr.Value + "'");
    }
}