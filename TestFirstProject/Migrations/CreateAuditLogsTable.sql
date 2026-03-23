-- Migration script for creating the audit_logs table
-- Run this script against your PostgreSQL database

CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    timestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    user_id VARCHAR(255) NOT NULL,
    user_name VARCHAR(255) NOT NULL,
    action VARCHAR(50) NOT NULL,
    entity_type VARCHAR(255) NOT NULL,
    entity_id VARCHAR(255) NOT NULL,
    old_values JSONB,
    new_values JSONB,
    additional_data JSONB,
    ip_address VARCHAR(45) NOT NULL,
    user_agent TEXT,
    correlation_id VARCHAR(50) NOT NULL,
    request_path VARCHAR(2048),
    request_method VARCHAR(10),
    response_status_code INTEGER,
    environment VARCHAR(50) NOT NULL,
    checksum VARCHAR(64) NOT NULL
);

-- Indexes for common query patterns
CREATE INDEX IF NOT EXISTS idx_audit_logs_timestamp ON audit_logs (timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON audit_logs (user_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_entity ON audit_logs (entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_correlation_id ON audit_logs (correlation_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_action ON audit_logs (action);

-- Add comment to table
COMMENT ON TABLE audit_logs IS 'Audit log table for tracking all system changes and user actions';
