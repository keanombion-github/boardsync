CREATE INDEX IF NOT EXISTS idx_boards_owner_id_created_at
    ON boards (owner_id, created_at DESC);

CREATE INDEX IF NOT EXISTS idx_columns_board_id_position
    ON columns (board_id, position);

CREATE INDEX IF NOT EXISTS idx_cards_column_id_position
    ON cards (column_id, position);
