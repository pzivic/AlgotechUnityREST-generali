USE OnBaseUTL
GO

-- 3BANK

-- Kreiramo sekvencu za prefiks
-- DROP SEQUENCE dbo.RefNoUPI
/*
CREATE SEQUENCE dbo.RefNoUPI AS int START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE dbo.RefNoUPE AS int START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE dbo.RefNoIP AS int START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE dbo.RefNoKUF AS int START WITH 1 INCREMENT BY 1;

SELECT CONCAT(FORMAT(NEXT VALUE FOR dbo.RefNoUPI, 'UPI-#'), '/2024') AS ref_no
SELECT CONCAT(FORMAT(NEXT VALUE FOR dbo.RefNoUPE, 'UPE-#'), '/2024')
SELECT CONCAT(FORMAT(NEXT VALUE FOR dbo.RefNoIP, 'IP-#'), '/2024')
SELECT CONCAT(FORMAT(NEXT VALUE FOR dbo.RefNoKUF, 'KUF-#'), '/2024')

GRANT UPDATE ON dbo.RefNoUPI TO web_access
*/

--- CKB

-- sednice management boarda
CREATE SEQUENCE dbo.RefNoMBD AS int START WITH 1 INCREMENT BY 1;
GRANT UPDATE ON dbo.RefNoUPI TO web_access

/*
CREATE SEQUENCE dbo.RefNoDocBarcode AS int START WITH 1400 INCREMENT BY 1 ;
CREATE SEQUENCE dbo.RefNoGroupBarcode AS int START WITH 1400 INCREMENT BY 1;

--GRANT UPDATE ON dbo.DocBarcode TO web_access
--GRANT UPDATE ON dbo.GroupBarcode TO web_access

DROP SEQUENCE dbo.RefNoDocBarcode 
DROP SEQUENCE dbo.RefNoGroupBarcode

--SELECT NEXT VALUE FOR RefNoDocBarcode AS seq


ALTER SEQUENCE RefNoDocBarcode RESTART WITH 5521;
ALTER SEQUENCE RefNoDocBarcode RESTART WITH 5521;
**/

-- Za deployment
USE OnBaseSBep6Unicode
GO

--Pokupimo poslenju [document handle]+1
SELECT TOP (1) itemnum+1 FROM hsi.itemdata ORDER BY itemnum DESC

USE OnBaseUTL
GO

--Kreiramo sekvencu [document handle]+1
CREATE SEQUENCE dbo.RefNo_DocBarcodeGroupBarcode AS int START WITH 5541 INCREMENT BY 1;

--DROP SEQUENCE dbo.RefNo_DocBarcodeGroupBarcode
