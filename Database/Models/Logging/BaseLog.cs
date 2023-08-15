using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Database.Models.Logging;

public class LogObject
{
	[Key]
	public long Id { get; set; }
	public DateTime Time { get; set; }

	[Column("log", TypeName = "jsonb")]
	public BaseLog Log { get; set; }
}

[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(MessageLog), 0)]
public abstract class BaseLog
{

}