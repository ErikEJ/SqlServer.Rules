CREATE XML SCHEMA COLLECTION [Production].[ManuInstructionsSchemaCollection]
    AS N'<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:t="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelManuInstructions" targetNamespace="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelManuInstructions" elementFormDefault="qualified">
  <xsd:element name="root">
    <xsd:complexType mixed="true">
      <xsd:complexContent mixed="true">
        <xsd:restriction base="xsd:anyType">
          <xsd:sequence>
            <xsd:element name="Location" maxOccurs="unbounded">
              <xsd:complexType mixed="true">
                <xsd:complexContent mixed="true">
                  <xsd:restriction base="xsd:anyType">
                    <xsd:sequence>
                      <xsd:element name="step" type="t:StepType" maxOccurs="unbounded" />
                    </xsd:sequence>
                    <xsd:attribute name="LocationID" type="xsd:integer" use="required" />
                    <xsd:attribute name="SetupHours" type="xsd:decimal" />
                    <xsd:attribute name="MachineHours" type="xsd:decimal" />
                    <xsd:attribute name="LaborHours" type="xsd:decimal" />
                    <xsd:attribute name="LotSize" type="xsd:decimal" />
                  </xsd:restriction>
                </xsd:complexContent>
              </xsd:complexType>
            </xsd:element>
          </xsd:sequence>
        </xsd:restriction>
      </xsd:complexContent>
    </xsd:complexType>
  </xsd:element>
  <xsd:complexType name="StepType" mixed="true">
    <xsd:complexContent mixed="true">
      <xsd:restriction base="xsd:anyType">
        <xsd:choice minOccurs="0" maxOccurs="unbounded">
          <xsd:element name="tool" type="xsd:string" />
          <xsd:element name="material" type="xsd:string" />
          <xsd:element name="blueprint" type="xsd:string" />
          <xsd:element name="specs" type="xsd:string" />
          <xsd:element name="diag" type="xsd:string" />
        </xsd:choice>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
</xsd:schema>';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Collection of XML schemas for the Instructions column in the Production.ProductModel table.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'XML SCHEMA COLLECTION', @level1name = N'ManuInstructionsSchemaCollection';


GO

