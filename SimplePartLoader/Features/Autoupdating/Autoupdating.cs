using System;

[AttributeUsage(AttributeTargets.Assembly)]
public class AutoupdaterID : Attribute
{
    string id;
    public AutoupdaterID() : this(string.Empty) { }
    public AutoupdaterID(string txt) { id = txt; }
    public string Value() { return id; }
}

[AttributeUsage(AttributeTargets.Assembly)]
public class AutoupdaterName : Attribute
{
    string name;
    public AutoupdaterName() : this(string.Empty) { }
    public AutoupdaterName(string txt) { name = txt; }
    public string Value() { return name; }
}

[AttributeUsage(AttributeTargets.Assembly)]
public class AutoupdaterVersion : Attribute
{
    string ver;
    public AutoupdaterVersion() : this(string.Empty) { }
    public AutoupdaterVersion(string txt) { ver = txt; }
    public string Value() { return ver; }
}

