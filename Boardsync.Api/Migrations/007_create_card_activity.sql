CREATE TABLE card_comments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    card_id UUID NOT NULL REFERENCES cards(id) ON DELETE CASCADE,
    author_id UUID NOT NULL REFERENCES users(id),
    body VARCHAR(2000) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT card_comments_body_not_blank CHECK (length(btrim(body)) > 0)
);

CREATE INDEX idx_card_comments_card_created ON card_comments (card_id, created_at, id);

CREATE TABLE card_reactions (
    card_id UUID NOT NULL REFERENCES cards(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id),
    emoji VARCHAR(16) NOT NULL,
    PRIMARY KEY (card_id, user_id, emoji)
);

CREATE INDEX idx_card_reactions_card ON card_reactions (card_id);
