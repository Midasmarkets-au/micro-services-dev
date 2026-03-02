DO
$do$
DECLARE
  cur CURSOR FOR SELECT * FROM core."_Supplement" WHERE "Type" = 243;
  row core."_Supplement"%ROWTYPE;
  new_verification_id bigint;
BEGIN
  OPEN cur;

  LOOP
    FETCH cur INTO row;
    EXIT WHEN NOT FOUND;

    INSERT INTO core."_Verification" ("PartyId", "Type", "Status", "Note")
    VALUES (row."RowId", 1, 1, 'Import from legacy database')
    RETURNING "Id" INTO new_verification_id;

    INSERT INTO core."_VerificationItem" ("VerificationId", "Category", "Content")
    VALUES (new_verification_id, 'document', json_build_object(
        'IdFront', '',
        'IdBack', '',
        'AddressDocument', '',
        'Other', row."Data"
        ));
  END LOOP;

  CLOSE cur;
END
$do$;