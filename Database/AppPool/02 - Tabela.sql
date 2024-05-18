-- Predrag Živić
-- 06.03.2024
-- Kreiranje tabela za app poll 


USE OnBaseUTL
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AppPool](
	[AppPoolID] [int] NOT NULL,
	[AppPoolIdentity] [varchar](50) NOT NULL,
	[AppPoolPassword] [varchar](50) NOT NULL,
	[AppPoolPasswordSalt] [varchar](5) NOT NULL,
	[AppPoolStatus] [tinyint] NOT NULL,
 CONSTRAINT [PK_AppPool] PRIMARY KEY CLUSTERED 
(
	[AppPoolID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AppPool] ADD  CONSTRAINT [DF_AppPool_AppPoolIsAvailable]  DEFAULT ((0)) FOR [AppPoolStatus]
GO


