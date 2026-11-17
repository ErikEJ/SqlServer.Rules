CREATE COLUMN MASTER KEY [CMK]
WITH (KEY_STORE_PROVIDER_NAME = N'MSSQL_CERTIFICATE_STORE', KEY_PATH = N'CurrentUser/my/cert');
GO
CREATE COLUMN ENCRYPTION KEY [CEK]
WITH VALUES (COLUMN_MASTER_KEY = [CMK], ALGORITHM = 'RSA_OAEP', ENCRYPTED_VALUE = 0x01);
GO
CREATE TABLE [dbo].[SensitiveColumns]
(
    [Unprotected] NVARCHAR(100) NULL,
    [Masked] NVARCHAR(100) MASKED WITH (FUNCTION = 'email()') NULL,
    [Encrypted] NVARCHAR(100) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK], ENCRYPTION_TYPE = RANDOMIZED, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NULL,
    [PublicReviewed] NVARCHAR(100) NULL,
    [NoneReviewed] NVARCHAR(100) NULL,
    [NoneInformationType] NVARCHAR(100) NULL,
    [RankedSensitive] NVARCHAR(100) NULL,
    [PublicOnly] NVARCHAR(100) NULL,
    [RankOnly] NVARCHAR(100) NULL,
    [InformationTypeOnly] NVARCHAR(100) NULL,
    [Unclassified] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_SensitiveColumns] PRIMARY KEY CLUSTERED ([Unclassified])
);
GO
ADD SENSITIVITY CLASSIFICATION TO [dbo].[SensitiveColumns].[Unprotected]
    WITH (LABEL = 'Confidential', INFORMATION_TYPE = 'Personal');
GO
ADD SENSITIVITY CLASSIFICATION TO [dbo].[SensitiveColumns].[Masked]
    WITH (LABEL = 'Confidential', INFORMATION_TYPE = 'Contact Info');
GO
ADD SENSITIVITY CLASSIFICATION TO [dbo].[SensitiveColumns].[Encrypted]
    WITH (LABEL = 'Highly Confidential', INFORMATION_TYPE = 'National ID');
GO
ADD SENSITIVITY CLASSIFICATION TO [dbo].[SensitiveColumns].[PublicReviewed]
    WITH (LABEL = 'Public', RANK = NONE);
GO
ADD SENSITIVITY CLASSIFICATION TO [dbo].[SensitiveColumns].[NoneReviewed]
    WITH (LABEL = 'Reviewed', RANK = NONE);
GO
ADD SENSITIVITY CLASSIFICATION TO [dbo].[SensitiveColumns].[NoneInformationType]
    WITH (INFORMATION_TYPE = 'Reviewed', RANK = NONE);
GO
ADD SENSITIVITY CLASSIFICATION TO [dbo].[SensitiveColumns].[RankedSensitive]
    WITH (LABEL = 'Confidential', INFORMATION_TYPE = 'Personal', RANK = HIGH);
GO
ADD SENSITIVITY CLASSIFICATION TO [dbo].[SensitiveColumns].[PublicOnly]
    WITH (LABEL = 'public');
GO
ADD SENSITIVITY CLASSIFICATION TO [dbo].[SensitiveColumns].[RankOnly]
    WITH (RANK = HIGH);
GO
ADD SENSITIVITY CLASSIFICATION TO [dbo].[SensitiveColumns].[InformationTypeOnly]
    WITH (INFORMATION_TYPE = 'Personal', RANK = HIGH);
