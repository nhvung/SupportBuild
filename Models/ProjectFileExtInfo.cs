using VSSystem.IO;
using VSSystem.IO.SourceCode;

class ProjectFileExtInfo : ProjectFileInfo
{
    public string Name { get { return base.AssemblyName; } }
    ExcludeCondition _ExcludeCondition;
    public ExcludeCondition ExcludeCondition { get { return _ExcludeCondition; } set { _ExcludeCondition = value; } }
    string _OutputPath;
    public string OutputPath { get { return _OutputPath; } set { _OutputPath = value; } }
    public ProjectFileExtInfo(string projectFilePath) : base(projectFilePath)
    {
        _ExcludeCondition = ExcludeCondition.PublishSourceExcludeCondition();
        _OutputPath = string.Empty;
    }
}