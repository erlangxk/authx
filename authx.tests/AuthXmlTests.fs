module AuthXmlTests

open System.IO
open Xunit
open authx
open System.Text

let toStream (s: string) =
    new MemoryStream(Encoding.UTF8.GetBytes(s))

let xml1 =
    """
  <authenticate>
  <statusCode>00</statusCode>
  <statusText>OK</statusText>
  <operatorId>1</operatorId>
  <memberId>691832</memberId>
  <memberCode>iOSDEV</memberCode>
  <testAccount>1</testAccount>
  <currency>RMB</currency>
  <language>en-us</language>
  <country>SG</country>
  <riskId>VIPG</riskId>
  <referrerId>RBTKMCH6</referrerId>
  <dob>01-01-1998</dob>
  <requesterIp>10.1.0.174</requesterIp>
  <datetime>2023-03-29T00:27:26.9484499+08:00</datetime>
  <loginIP>103.22.183.189</loginIP>
</authenticate>
"""

[<Fact>]

let ``test read xml ok`` () =
    let result = toStream xml1 |> AuthXml.parseOp1

    let claims =
        match result with
        | AuthXml.Success m -> Map.ofSeq m
        | _ -> Map.empty

    Assert.Equal("RMB", claims.[MyJwtClaims.currency].ToString())


let xml2 =
    """
<authenticate>
  <statusCode>MissingToken</statusCode>
  <statusText></statusText>
  <operatorId></operatorId>
  <memberId></memberId>
  <memberCode></memberCode>
  <testAccount></testAccount>
  <currency></currency>
  <language></language>
  <country></country>
  <riskId></riskId>
  <referrerId></referrerId>
  <dob></dob>
  <requesterIp>10.1.0.174</requesterIp>
  <datetime>2023-03-30T18:49:16.2035206+08:00</datetime>
</authenticate>
"""

[<Fact>]
let ``test read xml error`` () =
    let result = xml2 |> toStream |> AuthXml.parseOp1
    Assert.Equal(AuthXml.Failed "MissingToken", result)
