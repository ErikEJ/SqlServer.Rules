CREATE XML SCHEMA COLLECTION [Person].[AdditionalContactInfoSchemaCollection]
    AS N'<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:t="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo" targetNamespace="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo">
  <xsd:element name="AdditionalContactInfo">
    <xsd:complexType mixed="true">
      <xsd:complexContent mixed="true">
        <xsd:restriction base="xsd:anyType">
          <xsd:sequence>
            <xsd:any namespace="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactRecord http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes" minOccurs="0" maxOccurs="unbounded" />
          </xsd:sequence>
        </xsd:restriction>
      </xsd:complexContent>
    </xsd:complexType>
  </xsd:element>
</xsd:schema>
<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:t="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactRecord" targetNamespace="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactRecord">
  <xsd:element name="ContactRecord">
    <xsd:complexType mixed="true">
      <xsd:complexContent mixed="true">
        <xsd:restriction base="xsd:anyType">
          <xsd:choice minOccurs="0" maxOccurs="unbounded">
            <xsd:any namespace="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes" />
          </xsd:choice>
          <xsd:attribute name="date" type="xsd:date" />
        </xsd:restriction>
      </xsd:complexContent>
    </xsd:complexType>
  </xsd:element>
</xsd:schema>
<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:t="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes" targetNamespace="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes" elementFormDefault="qualified">
  <xsd:element name="eMail" type="t:eMailType" />
  <xsd:element name="facsimileTelephoneNumber" type="t:phoneNumberType" />
  <xsd:element name="homePostalAddress" type="t:addressType" />
  <xsd:element name="internationaliSDNNumber" type="t:phoneNumberType" />
  <xsd:element name="mobile" type="t:phoneNumberType" />
  <xsd:element name="pager" type="t:phoneNumberType" />
  <xsd:element name="physicalDeliveryOfficeName" type="t:addressType" />
  <xsd:element name="registeredAddress" type="t:addressType" />
  <xsd:element name="telephoneNumber" type="t:phoneNumberType" />
  <xsd:element name="telexNumber" type="t:phoneNumberType" />
  <xsd:complexType name="addressType">
    <xsd:complexContent>
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:element name="Street" type="xsd:string" maxOccurs="2" />
          <xsd:element name="City" type="xsd:string" />
          <xsd:element name="StateProvince" type="xsd:string" />
          <xsd:element name="PostalCode" type="xsd:string" minOccurs="0" />
          <xsd:element name="CountryRegion" type="xsd:string" />
          <xsd:element name="SpecialInstructions" type="t:specialInstructionsType" minOccurs="0" />
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
  <xsd:complexType name="eMailType">
    <xsd:complexContent>
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:element name="eMailAddress" type="xsd:string" />
          <xsd:element name="SpecialInstructions" type="t:specialInstructionsType" minOccurs="0" />
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
  <xsd:complexType name="phoneNumberType">
    <xsd:complexContent>
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:element name="number">
            <xsd:simpleType>
              <xsd:restriction base="xsd:string">
                <xsd:pattern value="[0-9\(\)\-]*" />
              </xsd:restriction>
            </xsd:simpleType>
          </xsd:element>
          <xsd:element name="SpecialInstructions" type="t:specialInstructionsType" minOccurs="0" />
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
  <xsd:complexType name="specialInstructionsType" mixed="true">
    <xsd:complexContent mixed="true">
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:any namespace="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes" minOccurs="0" maxOccurs="unbounded" />
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
</xsd:schema>';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Collection of XML schemas for the AdditionalContactInfo column in the Person.Contact table.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'XML SCHEMA COLLECTION', @level1name = N'AdditionalContactInfoSchemaCollection';


GO

