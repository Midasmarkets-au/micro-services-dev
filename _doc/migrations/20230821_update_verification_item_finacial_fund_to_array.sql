UPDATE core."_VerificationItem" vi
SET "Content" = jsonb_set(
        "Content"::jsonb,
        '{fund}',
        ('[' || ("Content"->'fund') || ']')::jsonb
    )
WHERE jsonb_typeof("Content"::jsonb->'fund') = 'string'
  AND vi."Category"='financial';

  