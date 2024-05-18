-- Predrag Živić
-- 06.03.2024
-- Dodavanje servisnog naloga u app.pool ime usera
-- Cilj je da obezdedimo nesmetn rad API koji concurent userima ograničava sesiju

-- Integratio Guide
/*
Concurrent Client License
In the case of an API connection that is connected and then disconnected within 1 minute, that 
license will not be available for another user to consume until 4 minutes after the connection is properly 
disconnected, bringing the total time that the license was consumed to 5 minutes. 

In the case of an API connection that is connected and then disconnected after 5 hours of 
usage, that license will be released and immediately available for the next user to consume after the 
connection is properly disconnected. 
*/


USE OnBaseUTL
GO

--INSERT dbo.AppPool (AppPoolID, AppPoolIdentity, AppPoolPassword, AppPoolPasswordSalt, AppPoolStatus) VALUES (1, N'service_unityapi_named', N'aSd3#12gTi', N'DW2z', 1)

INSERT dbo.AppPool (AppPoolID, AppPoolIdentity, AppPoolPassword, AppPoolPasswordSalt, AppPoolStatus) VALUES (1, N'service_unityapi_1', N'aSd3#12gTi', N'DW2z', 1)
INSERT dbo.AppPool (AppPoolID, AppPoolIdentity, AppPoolPassword, AppPoolPasswordSalt, AppPoolStatus) VALUES (2, N'service_unityapi_2', N'aSd3#12gTi', N'DW2z', 1)
INSERT dbo.AppPool (AppPoolID, AppPoolIdentity, AppPoolPassword, AppPoolPasswordSalt, AppPoolStatus) VALUES (3, N'service_unityapi_3', N'aSd3#12gTi', N'DW2z', 1)


