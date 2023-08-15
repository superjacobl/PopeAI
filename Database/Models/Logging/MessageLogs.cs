using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Database.Models.Logging;

[JsonDerivedType(typeof(MessageDeletionLog), 0)]
public abstract class MessageLog : BaseLog
{
	public long MessageId { get; set; }
	public long AuthorId { get; set; }
}

public class MessageDeletionLog : MessageLog
{
	public long DeleterId { get; set; }
	public string MessageContent { get; set; }
}