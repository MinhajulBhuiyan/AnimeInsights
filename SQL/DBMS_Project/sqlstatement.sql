--voiceActor Anime
SELECT va.Id, va.Name, va.Gender, va.DOB
FROM VoiceActors va
INNER JOIN AnimeVoiceActorJoins avaj ON va.Id = avaj.VoiceActorId
INNER JOIN Animes a ON avaj.AnimeId = a.Id
WHERE a.Id = :animeId;

--Director Anime
SELECT 
    d."Id", 
    d."Name", 
    d."Gender", 
    d."DOB"
FROM 
    "SYSTEM"."Animes" a
JOIN 
    "SYSTEM"."Directors" d ON a."DirectorId" = d."Id"
WHERE 
    a."Id" = :animeId;

CREATE OR REPLACE TRIGGER SetDefaultDirectorId
BEFORE INSERT OR UPDATE ON "SYSTEM"."Animes"
FOR EACH ROW
BEGIN
    IF :NEW."DirectorId" IS NULL THEN
        :NEW."DirectorId" := 1;
    END IF;
END;
/
