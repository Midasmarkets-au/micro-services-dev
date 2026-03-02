DO
$$
DECLARE
    _tbl text;
    _col text;
    _schema text;
BEGIN
    FOR _schema, _tbl, _col IN (
        SELECT table_schema, table_name, column_name
        FROM information_schema.columns
        WHERE (data_type = 'timestamp without time zone' OR data_type = 'timestamp(0) without time zone')
        AND table_schema in( 'trd', 'cms','acct','sto','core','auth','rpt','identity')
    )
    LOOP
        EXECUTE format(
            'ALTER TABLE %I.%I ALTER COLUMN %I TYPE timestamp with time zone USING %I AT TIME ZONE ''UTC'';',
            _schema,_tbl, _col, _col
        );
    END LOOP;
END;
$$