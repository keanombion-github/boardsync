CREATE TABLE card_attachments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    card_id UUID NOT NULL REFERENCES cards(id) ON DELETE CASCADE,
    added_by UUID NOT NULL REFERENCES users(id),
    label VARCHAR(120) NOT NULL,
    url VARCHAR(2048) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT card_attachments_label_not_blank CHECK (length(btrim(label)) > 0)
);

CREATE INDEX idx_card_attachments_card_created
    ON card_attachments (card_id, created_at, id);
