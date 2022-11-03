<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FileContents.aspx.cs" Inherits="XmlFileCompare.FileContents" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>File Content Compare</title>
    <link href="bootstrap.min.css" rel="stylesheet" />
<style>
body {
width: 100%;
margin: 5px;
}
.labelstyleNew
{
    font-size: 1.125em;
    font-weight: bold;
    line-height: 1.5;
}
.table-condensed tr th {
border: 0px solid #6e7bd9;
color: white;
background-color: #6e7bd9;
}

.table-condensed, .table-condensed tr {
border: 0px solid #000;
}
td {
border: 1px solid #000;
}
tr:nth-child(even) {
background: #e7e3ff
}

tr:nth-child(odd) {
background: #fff;
}
 .labelstyle {
          color:black;
          font-family:'Arial';
          background-color:snow;
          font-style:normal;
        }
 .c-error { 
  
  padding: 10px !important;
  border-radius: 0 !important;
  position: relative; 
  display: inline-block !important;
  margin-top: 10px;
}
</style>
</head>
<body>
    
   <form id="form1" runat="server">
       <div>
        <label class="labelstyleNew" for ="ddFileNames">
            FileNames:
        </label>
        
        <asp:dropdownlist   id="ddFileNames" CssClass="dropdown-menu form-control"  runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddFileNames_SelectedIndexChanged"
             Width="120px" BackColor="white"  Font-Names="Arial" />
      </div>
       <div id="divDetails" runat="server">
            <br />
           <div>
                <asp:GridView ID="grdViewDelete" runat="server" HeaderStyle-BackColor="#ca6048" HeaderStyle-ForeColor="White"
                    RowStyle-BackColor="#EEDEDA" AlternatingRowStyle-BackColor="#ffaa95" AlternatingRowStyle-ForeColor="#000"
                    OnRowCreated="grdViewDelete_RowCreated"
                                    Caption='<table border="1" width="100%" cellpadding="0" cellspacing="0" style="font-weight: bold;background-color: coral;"><tr style="background-color: #dd5035;"><td>Delete</td></tr></table>'>
                </asp:GridView>

            </div>
          
            
                <br />
           <div>
        
                <asp:GridView ID="grdViewUpdate" runat="server" OnRowDataBound="grdViewUpdate_RowDataBound" HeaderStyle-BackColor="#0431C8 " HeaderStyle-ForeColor="White"
                    RowStyle-BackColor="White" AlternatingRowStyle-BackColor="#cbeafb" AlternatingRowStyle-ForeColor="#000" 
                    OnRowCreated="grdViewUpdate_RowCreated"
                         Caption='<table border="1" width="100%" cellpadding="0" cellspacing="0" bgcolor="#0431C8"  style="font-weight: bold;background-color: coral;"><tr style="background-color: #383ab8;"><td>Update</td></tr></table>'>
                
                </asp:GridView>
            </div>
            <br />
             <div>
                <asp:GridView ID="grdViewInsert" runat="server"  GridLines="Both" CssClass="table-condensed table-hover table-bordered"
                    OnRowCreated="grdViewInsert_RowCreated"
                         Caption='<table border="1" width="100%" cellpadding="0" cellspacing="0" bgcolor="#0431C8" style="font-weight: bold;background-color: coral;"><tr style="background-color: #5750d2;"><td>Insert </td></tr></table>'>
                
                </asp:GridView>
           </div>
             
                <br />  
            
           <br  />
             <div>
        
                <asp:GridView ID="grdViewInsertandDelete" runat="server" HeaderStyle-BackColor="#088F8F" HeaderStyle-ForeColor="White"
                RowStyle-BackColor="White" AlternatingRowStyle-BackColor= "#92dba4" AlternatingRowStyle-ForeColor="#000"
                    OnRowCreated="grdViewInsertandDelete_RowCreated"
                Caption='<table border="1" width="100%"  cellpadding="0" cellspacing="0"   style="font-weight: bold;background-color: coral;"><tr style="background-color: #37a8a8;"><td>Insert & Delete</td></tr></table>'>
                   
                </asp:GridView>
            </div>
        </div>
       <div>
           <asp:Label ID="labelErrorMsg" Visible="false" runat="server" CssClass="c-error" Text="Label" Font-Size="Large" style="color: red"></asp:Label>

       </div>
    </form>
</body>
</html>
