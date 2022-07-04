using IdGen;

namespace PopeAI.Database.Managers;

public class IdManager
{
    public IdGenerator Generator { get; set; }

    public IdManager(int threadid = 0)
    {
        // Fun fact: This is the exact moment that SpookVooper was terminated
        // which led to the development of Valour becoming more than just a side
        // project. Viva la Vooperia.
        var epoch = new DateTime(2021, 1, 11, 4, 37, 0);

        var structure = new IdStructure(45, 10, 8);

        var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));

        Generator = new IdGenerator(threadid, options);
    }

    public long Generate()
    {
        return (long)Generator.CreateId();
    }
}