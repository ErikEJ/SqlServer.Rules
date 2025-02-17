CREATE XML SCHEMA COLLECTION [Person].[IndividualSurveySchemaCollection]
    AS N'<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:t="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey" targetNamespace="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey" elementFormDefault="qualified">
  <xsd:element name="IndividualSurvey">
    <xsd:complexType>
      <xsd:complexContent>
        <xsd:restriction base="xsd:anyType">
          <xsd:sequence>
            <xsd:element name="TotalPurchaseYTD" type="xsd:decimal" minOccurs="0" />
            <xsd:element name="DateFirstPurchase" type="xsd:date" minOccurs="0" />
            <xsd:element name="BirthDate" type="xsd:date" minOccurs="0" />
            <xsd:element name="MaritalStatus" type="xsd:string" minOccurs="0" />
            <xsd:element name="YearlyIncome" type="t:SalaryType" minOccurs="0" />
            <xsd:element name="Gender" type="xsd:string" minOccurs="0" />
            <xsd:element name="TotalChildren" type="xsd:int" minOccurs="0" />
            <xsd:element name="NumberChildrenAtHome" type="xsd:int" minOccurs="0" />
            <xsd:element name="Education" type="xsd:string" minOccurs="0" />
            <xsd:element name="Occupation" type="xsd:string" minOccurs="0" />
            <xsd:element name="HomeOwnerFlag" type="xsd:string" minOccurs="0" />
            <xsd:element name="NumberCarsOwned" type="xsd:int" minOccurs="0" />
            <xsd:element name="Hobby" type="xsd:string" minOccurs="0" maxOccurs="unbounded" />
            <xsd:element name="CommuteDistance" type="t:MileRangeType" minOccurs="0" />
            <xsd:element name="Comments" type="xsd:string" minOccurs="0" />
          </xsd:sequence>
        </xsd:restriction>
      </xsd:complexContent>
    </xsd:complexType>
  </xsd:element>
  <xsd:simpleType name="MileRangeType">
    <xsd:restriction base="xsd:string">
      <xsd:enumeration value="0-1 Miles" />
      <xsd:enumeration value="1-2 Miles" />
      <xsd:enumeration value="2-5 Miles" />
      <xsd:enumeration value="5-10 Miles" />
      <xsd:enumeration value="10+ Miles" />
    </xsd:restriction>
  </xsd:simpleType>
  <xsd:simpleType name="SalaryType">
    <xsd:restriction base="xsd:string">
      <xsd:enumeration value="0-25000" />
      <xsd:enumeration value="25001-50000" />
      <xsd:enumeration value="50001-75000" />
      <xsd:enumeration value="75001-100000" />
      <xsd:enumeration value="greater than 100000" />
    </xsd:restriction>
  </xsd:simpleType>
</xsd:schema>';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Collection of XML schemas for the Demographics column in the Person.Person table.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'XML SCHEMA COLLECTION', @level1name = N'IndividualSurveySchemaCollection';


GO

