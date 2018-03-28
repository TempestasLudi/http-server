using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace com.tempestasludi.c.http_source.util
{
  /// <summary>
  /// Utility class for XML functionality.
  /// </summary>
  public static class Xml
  {
    /// <summary>
    /// Loads a document and validates it with a schema.
    /// </summary>
    /// <param name="documentPath">The document to load.</param>
    /// <param name="schemaPath">The path of the schema to validate against.</param>
    /// <returns>The loaded document.</returns>
    /// <exception cref="XmlSchemaValidationException">Thrown when the document is not valid according to the schema.</exception>
    public static XDocument LoadValidate(string documentPath, string schemaPath)
    {
      var schemaSet = new XmlSchemaSet();
      var schemaReader = new XmlTextReader(schemaPath);
      schemaSet.Add("", schemaReader);

      var document = XDocument.Load(documentPath);
      try
      {
        document.Validate(schemaSet, null);
      }
      catch (XmlSchemaValidationException e)
      {
        throw new XmlSchemaValidationException("Error while validating xml document " + documentPath, e);
      }

      return document;
    }
  }
}