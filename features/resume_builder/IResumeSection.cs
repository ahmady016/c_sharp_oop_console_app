namespace ResumesBuilder;

public interface IResumeSection
{
    string Title { get; }
    bool IsEmpty { get; }
    string Render();
}
