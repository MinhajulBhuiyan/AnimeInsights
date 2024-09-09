-- Drop database commands

-- Connect to the CDB as SYSDBA
CONNECT / AS SYSDBA;

-- Close the PDB if it is open
ALTER PLUGGABLE DATABASE Andb CLOSE IMMEDIATE;

-- Drop the PDB including its datafiles
DROP PLUGGABLE DATABASE Andb INCLUDING DATAFILES;


-----------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------

-- Connect to the CDB
-- CONNECT / AS SYSDBA;

-- Create a new Pluggable Database (PDB) with the desired service name
CREATE PLUGGABLE DATABASE Andb ADMIN USER me IDENTIFIED BY 123
  FILE_NAME_CONVERT=('pdbseed', 'Andb');

-- Open the new PDB
ALTER PLUGGABLE DATABASE Andb OPEN;

-- Connect to the new PDB
-- CONNECT me@Andb/123

-- Perform further configuration or object creation in the new PDB

-- Exit the PDB
-- EXIT;