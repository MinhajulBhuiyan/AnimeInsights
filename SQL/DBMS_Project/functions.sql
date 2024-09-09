--remove duplicate anime
CREATE OR REPLACE FUNCTION RemoveDuplicateAnimes RETURN VARCHAR2 AS
    removed_count NUMBER := 0;
BEGIN
    FOR r IN (
        SELECT "Title", MAX("Id") AS KeepId
        FROM "Animes"
        GROUP BY "Title"
        HAVING COUNT(*) > 1
    ) LOOP
        -- For each duplicate group, delete the duplicates and increment the removed_count
        DELETE FROM "Animes"
        WHERE "Title" = r."Title"
        AND "Id" != r.KeepId;
        
        -- Increment the removed count by the number of rows deleted
        removed_count := removed_count + SQL%ROWCOUNT;
    END LOOP;

    RETURN removed_count || ' duplicate animes removed';
END;
/
--Find username by id
CREATE OR REPLACE FUNCTION GetUserNameByUserId(
    p_UserId IN NVARCHAR2
)
RETURN NVARCHAR2
AS
    v_UserName NVARCHAR2(256);
BEGIN
    SELECT UserName INTO v_UserName
    FROM "AspNetUsers"
    WHERE Id = p_UserId;

    RETURN v_UserName;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN NULL;
END GetUserNameByUserId;
/
--assign director
CREATE OR REPLACE PROCEDURE AssignDirectorsToAnimes AS
BEGIN
    FOR i IN 1..500 LOOP
        UPDATE "Animes"
        SET "DirectorId" = DBMS_RANDOM.VALUE(1, 496)
        WHERE "Id" = i;
    END LOOP;
    
    COMMIT;
END;
/
BEGIN
    AssignDirectorsToAnimes;
END;
/



