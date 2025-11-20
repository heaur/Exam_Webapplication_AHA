// src/components/ConfirmDialog.tsx
// --------------------------------
// Generic confirm dialog component.
// Can be used for delete actions or any dangerous operation.

import React from "react";

interface ConfirmDialogProps {
  open: boolean;              // Whether the dialog is visible
  title?: string;             // Optional title text
  message: string;            // Main message / question
  confirmText?: string;       // Text for confirm button
  cancelText?: string;        // Text for cancel button
  onConfirm: () => void;      // Called when user confirms
  onCancel: () => void;       // Called when user cancels / closes
}

const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
  open,
  title = "Confirm",
  message,
  confirmText = "Yes",
  cancelText = "Cancel",
  onConfirm,
  onCancel,
}) => {
  if (!open) return null;

  return (
    <div className="confirm-dialog-backdrop">
      <div className="confirm-dialog">
        <h2 className="confirm-dialog-title">{title}</h2>
        <p className="confirm-dialog-message">{message}</p>

        <div className="confirm-dialog-actions">
          <button
            type="button"
            className="btn btn-secondary"
            onClick={onCancel}
          >
            {cancelText}
          </button>
          <button
            type="button"
            className="btn btn-danger"
            onClick={onConfirm}
          >
            {confirmText}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmDialog;
