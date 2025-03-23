CREATE VIEW dbo.Albums
AS
SELECT dbo.Album.Title, dbo.Artist.Name
FROM     dbo.Album INNER JOIN
                  dbo.Artist ON dbo.Album.ArtistId = dbo.Artist.ArtistId;

GO
