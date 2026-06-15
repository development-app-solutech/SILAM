-- Script de création des tables pour PostgreSQL
-- Exécuter ce script si les migrations automatiques ne fonctionnent pas

CREATE TABLE IF NOT EXISTS lab_results (
    id SERIAL PRIMARY KEY,
    raw_message TEXT,
    patient_id VARCHAR(50),
    last_name VARCHAR(100),
    first_name VARCHAR(100),
    birth_date TIMESTAMP,
    sex VARCHAR(10),
    specimen_id VARCHAR(50),
    ordering_physician VARCHAR(100),
    received_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS test_results (
    id SERIAL PRIMARY KEY,
    lab_result_id INTEGER NOT NULL,
    specimen_id VARCHAR(50) NOT NULL,
    universal_test_id VARCHAR(50) NOT NULL,
    test_name VARCHAR(100) NOT NULL,
    data_measurement_value VARCHAR(50) NOT NULL,
    units VARCHAR(20),
    reference_ranges VARCHAR(50),
    result_abnormal_flags VARCHAR(10),
    numeric_value DECIMAL(10,3),
    is_numeric BOOLEAN NOT NULL,
    result_status VARCHAR(10),
    FOREIGN KEY (lab_result_id) REFERENCES lab_results(id) ON DELETE CASCADE
);

-- Index pour améliorer les performances
CREATE INDEX IF NOT EXISTS idx_lab_results_patient_id ON lab_results(patient_id);
CREATE INDEX IF NOT EXISTS idx_lab_results_received_at ON lab_results(received_at);
CREATE INDEX IF NOT EXISTS idx_test_results_lab_result_id ON test_results(lab_result_id);
CREATE INDEX IF NOT EXISTS idx_test_results_test_name ON test_results(test_name);