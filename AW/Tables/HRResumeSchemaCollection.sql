CREATE XML SCHEMA COLLECTION [HumanResources].[HRResumeSchemaCollection]
    AS N'<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:t="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume" targetNamespace="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume" elementFormDefault="qualified">
  <xsd:element name="Address" type="t:AddressType" />
  <xsd:element name="Education" type="t:EducationType" />
  <xsd:element name="Employment" type="t:EmploymentType" />
  <xsd:element name="Location" type="t:LocationType" />
  <xsd:element name="Name" type="t:NameType" />
  <xsd:element name="Resume" type="t:ResumeType" />
  <xsd:element name="Telephone" type="t:TelephoneType" />
  <xsd:complexType name="AddressType">
    <xsd:complexContent>
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:element name="Addr.Type" type="xsd:string" />
          <xsd:element name="Addr.OrgName" type="xsd:string" minOccurs="0" />
          <xsd:element name="Addr.Street" type="xsd:string" maxOccurs="unbounded" />
          <xsd:element name="Addr.Location">
            <xsd:complexType>
              <xsd:complexContent>
                <xsd:restriction base="xsd:anyType">
                  <xsd:sequence>
                    <xsd:element ref="t:Location" />
                  </xsd:sequence>
                </xsd:restriction>
              </xsd:complexContent>
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="Addr.PostalCode" type="xsd:string" />
          <xsd:element name="Addr.Telephone" minOccurs="0">
            <xsd:complexType>
              <xsd:complexContent>
                <xsd:restriction base="xsd:anyType">
                  <xsd:sequence>
                    <xsd:element ref="t:Telephone" maxOccurs="unbounded" />
                  </xsd:sequence>
                </xsd:restriction>
              </xsd:complexContent>
            </xsd:complexType>
          </xsd:element>
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
  <xsd:complexType name="EducationType">
    <xsd:complexContent>
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:element name="Edu.Level" type="xsd:string" />
          <xsd:element name="Edu.StartDate" type="xsd:date" />
          <xsd:element name="Edu.EndDate" type="xsd:date" />
          <xsd:element name="Edu.Degree" type="xsd:string" minOccurs="0" />
          <xsd:element name="Edu.Major" type="xsd:string" minOccurs="0" />
          <xsd:element name="Edu.Minor" type="xsd:string" minOccurs="0" />
          <xsd:element name="Edu.GPA" type="xsd:string" minOccurs="0" />
          <xsd:element name="Edu.GPAAlternate" type="xsd:decimal" minOccurs="0" />
          <xsd:element name="Edu.GPAScale" type="xsd:decimal" minOccurs="0" />
          <xsd:element name="Edu.School" type="xsd:string" minOccurs="0" />
          <xsd:element name="Edu.Location" minOccurs="0">
            <xsd:complexType>
              <xsd:complexContent>
                <xsd:restriction base="xsd:anyType">
                  <xsd:sequence>
                    <xsd:element ref="t:Location" />
                  </xsd:sequence>
                </xsd:restriction>
              </xsd:complexContent>
            </xsd:complexType>
          </xsd:element>
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
  <xsd:complexType name="EmploymentType">
    <xsd:complexContent>
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:element name="Emp.StartDate" type="xsd:date" minOccurs="0" />
          <xsd:element name="Emp.EndDate" type="xsd:date" minOccurs="0" />
          <xsd:element name="Emp.OrgName" type="xsd:string" />
          <xsd:element name="Emp.JobTitle" type="xsd:string" />
          <xsd:element name="Emp.Responsibility" type="xsd:string" />
          <xsd:element name="Emp.FunctionCategory" type="xsd:string" minOccurs="0" />
          <xsd:element name="Emp.IndustryCategory" type="xsd:string" minOccurs="0" />
          <xsd:element name="Emp.Location" minOccurs="0">
            <xsd:complexType>
              <xsd:complexContent>
                <xsd:restriction base="xsd:anyType">
                  <xsd:sequence>
                    <xsd:element ref="t:Location" />
                  </xsd:sequence>
                </xsd:restriction>
              </xsd:complexContent>
            </xsd:complexType>
          </xsd:element>
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
  <xsd:complexType name="LocationType">
    <xsd:complexContent>
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:element name="Loc.CountryRegion" type="xsd:string" />
          <xsd:element name="Loc.State" type="xsd:string" minOccurs="0" />
          <xsd:element name="Loc.City" type="xsd:string" minOccurs="0" />
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
  <xsd:complexType name="NameType">
    <xsd:complexContent>
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:element name="Name.Prefix" type="xsd:string" minOccurs="0" />
          <xsd:element name="Name.First" type="xsd:string" />
          <xsd:element name="Name.Middle" type="xsd:string" minOccurs="0" />
          <xsd:element name="Name.Last" type="xsd:string" />
          <xsd:element name="Name.Suffix" type="xsd:string" minOccurs="0" />
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
  <xsd:complexType name="ResumeType">
    <xsd:complexContent>
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:element ref="t:Name" />
          <xsd:element name="Skills" type="xsd:string" minOccurs="0" />
          <xsd:element ref="t:Employment" maxOccurs="unbounded" />
          <xsd:element ref="t:Education" maxOccurs="unbounded" />
          <xsd:element ref="t:Address" maxOccurs="unbounded" />
          <xsd:element ref="t:Telephone" minOccurs="0" />
          <xsd:element name="EMail" type="xsd:string" minOccurs="0" />
          <xsd:element name="WebSite" type="xsd:string" minOccurs="0" />
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
  <xsd:complexType name="TelephoneType">
    <xsd:complexContent>
      <xsd:restriction base="xsd:anyType">
        <xsd:sequence>
          <xsd:element name="Tel.Type" type="xsd:anyType" minOccurs="0" />
          <xsd:element name="Tel.IntlCode" type="xsd:int" minOccurs="0" />
          <xsd:element name="Tel.AreaCode" type="xsd:int" minOccurs="0" />
          <xsd:element name="Tel.Number" type="xsd:string" />
          <xsd:element name="Tel.Extension" type="xsd:int" minOccurs="0" />
        </xsd:sequence>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
</xsd:schema>';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Collection of XML schemas for the Resume column in the HumanResources.JobCandidate table.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'XML SCHEMA COLLECTION', @level1name = N'HRResumeSchemaCollection';


GO

