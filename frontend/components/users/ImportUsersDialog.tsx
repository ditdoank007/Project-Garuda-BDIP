"use client";

import { useRef, useState } from "react";
import {
  CheckCircle2,
  FileUp,
  Loader2,
  RotateCcw,
  Upload,
  XCircle,
} from "lucide-react";
import { toast } from "sonner";

import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

import {
  executeSynologyCsvImport,
  previewSynologyCsv,
  uploadSynologyCsv,
} from "@/lib/api/user-import";

import type {
  SynologyImportPreview,
  SynologyImportResult,
} from "@/types/user-import";

interface ImportUsersDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

function getErrorMessage(
  error: unknown,
  fallback: string,
): string {
  if (
    typeof error === "object" &&
    error !== null &&
    "response" in error
  ) {
    const response = error as {
      response?: {
        data?: {
          message?: string;
          title?: string;
          detail?: string;
        };
      };
    };

    return (
      response.response?.data?.message ??
      response.response?.data?.detail ??
      response.response?.data?.title ??
      fallback
    );
  }

  return fallback;
}

function SummaryCard({
  label,
  value,
}: {
  label: string;
  value: number;
}) {
  return (
    <div className="rounded-lg border bg-background p-3">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="mt-1 text-2xl font-semibold tabular-nums">
        {value}
      </p>
    </div>
  );
}

export default function ImportUsersDialog({
  open,
  onOpenChange,
}: ImportUsersDialogProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [selectedFile, setSelectedFile] =
    useState<File | null>(null);
  const [uploadId, setUploadId] = useState("");
  const [preview, setPreview] =
    useState<SynologyImportPreview | null>(null);
  const [initialPassword, setInitialPassword] = useState("");
  const [uploading, setUploading] = useState(false);
  const [importing, setImporting] = useState(false);
  const [result, setResult] =
    useState<SynologyImportResult | null>(null);

  function resetState() {
    setSelectedFile(null);
    setUploadId("");
    setPreview(null);
    setInitialPassword("");
    setUploading(false);
    setImporting(false);
    setResult(null);

    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  }

  function handleOpenChange(nextOpen: boolean) {
    onOpenChange(nextOpen);

    if (!nextOpen) {
      resetState();
    }
  }

  async function handleUploadAndPreview() {
    if (!selectedFile) {
      toast.error("Please select a CSV file first.");
      return;
    }

    if (!selectedFile.name.toLowerCase().endsWith(".csv")) {
      toast.error("Only CSV files are supported.");
      return;
    }

    try {
      setUploading(true);
      setPreview(null);
      setResult(null);

      const upload = await uploadSynologyCsv(selectedFile);
      setUploadId(upload.uploadId);

      const previewResponse = await previewSynologyCsv(
        upload.uploadId,
      );

      setPreview(previewResponse);
      toast.success("CSV uploaded and preview generated.");
    } catch (error) {
      console.error("CSV upload or preview failed:", error);
      toast.error(
        getErrorMessage(
          error,
          "Failed to upload or preview CSV file.",
        ),
      );
    } finally {
      setUploading(false);
    }
  }

  async function handleExecuteImport() {
    if (!uploadId || !preview) {
      toast.error("Upload and preview the CSV first.");
      return;
    }

    if (!initialPassword) {
      toast.error("Initial password is required.");
      return;
    }

    if (initialPassword.length < 8) {
      toast.error(
        "Initial password must contain at least 8 characters.",
      );
      return;
    }

    const confirmed = window.confirm(
      [
        `Import ${preview.newUsers} new user(s) into LDAP?`,
        "",
        "Existing users will be skipped.",
        "Group memberships from the CSV will be added.",
        "This action cannot be undone automatically.",
      ].join("\n"),
    );

    if (!confirmed) {
      return;
    }

    try {
      setImporting(true);

      const importResult = await executeSynologyCsvImport(
        uploadId,
        initialPassword,
      );

      setResult(importResult);

      toast.success(
        `Import completed. ${importResult.createdUsers} user(s) created.`,
      );
    } catch (error) {
      console.error("CSV import failed:", error);
      toast.error(
        getErrorMessage(
          error,
          "Failed to import Synology users.",
        ),
      );
    } finally {
      setImporting(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="flex max-h-[92vh] w-[calc(100%-2rem)] max-w-6xl flex-col overflow-hidden p-0 sm:max-w-6xl">
        <DialogHeader className="border-b px-6 py-5 pr-14">
          <DialogTitle>Import Users from Synology CSV</DialogTitle>
        </DialogHeader>

        <div className="min-h-0 flex-1 overflow-y-auto px-6 py-5">
          {!result ? (
            <div className="space-y-5">
              <div className="rounded-lg border bg-muted/30 p-4">
                <p className="text-sm font-medium">Import workflow</p>
                <p className="mt-1 text-sm text-muted-foreground">
                  Upload an exported Synology user CSV, review the
                  preview, then import new users into OpenLDAP.
                  Existing usernames will be skipped.
                </p>
              </div>

              <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_auto] lg:items-end">
                <div className="space-y-2">
                  <Label htmlFor="synology-csv-file">
                    Synology user CSV file
                  </Label>

                  <Input
                    ref={fileInputRef}
                    id="synology-csv-file"
                    type="file"
                    accept=".csv,text/csv"
                    disabled={uploading || importing}
                    onChange={(event) => {
                      const file = event.target.files?.[0] ?? null;
                      setSelectedFile(file);
                      setUploadId("");
                      setPreview(null);
                      setResult(null);
                    }}
                  />

                  {selectedFile && (
                    <p className="text-xs text-muted-foreground">
                      Selected: {selectedFile.name} ·{" "}
                      {(selectedFile.size / 1024).toFixed(1)} KB
                    </p>
                  )}
                </div>

                <Button
                  type="button"
                  variant="outline"
                  disabled={!selectedFile || uploading || importing}
                  onClick={() => void handleUploadAndPreview()}
                >
                  {uploading ? (
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  ) : (
                    <FileUp className="mr-2 h-4 w-4" />
                  )}
                  Upload and Preview
                </Button>
              </div>

              {preview && (
                <div className="space-y-5 rounded-lg border bg-muted/10 p-4">
                  <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                    <div>
                      <h3 className="font-semibold">Import Preview</h3>
                      <p className="mt-1 text-sm text-muted-foreground">
                        Review the summary before importing users.
                      </p>
                    </div>

                    <Badge variant="outline">
                      {preview.groupsFound.length} group(s) found
                    </Badge>
                  </div>

                  <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
                    <SummaryCard
                      label="Total rows"
                      value={preview.totalRows}
                    />
                    <SummaryCard
                      label="New users"
                      value={preview.newUsers}
                    />
                    <SummaryCard
                      label="Existing users"
                      value={preview.existingUsers}
                    />
                    <SummaryCard
                      label="Disabled users"
                      value={preview.disabledUsers}
                    />
                  </div>

                  <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
                    <SummaryCard
                      label="Valid rows"
                      value={preview.validRows}
                    />
                    <SummaryCard
                      label="Duplicate usernames"
                      value={preview.duplicateUsernames}
                    />
                    <SummaryCard
                      label="Without email"
                      value={preview.usersWithoutEmail}
                    />
                  </div>

                  <div className="space-y-2">
                    <p className="text-sm font-medium">Groups found</p>
                    <div className="flex flex-wrap gap-2">
                      {preview.groupsFound.length === 0 ? (
                        <span className="text-sm text-muted-foreground">
                          No groups found in this CSV.
                        </span>
                      ) : (
                        preview.groupsFound.map((group) => (
                          <Badge key={group} variant="secondary">
                            {group}
                          </Badge>
                        ))
                      )}
                    </div>
                  </div>

                  {preview.newUsers === 0 && (
                    <div className="rounded-lg border bg-muted/30 p-3 text-sm text-muted-foreground">
                      No new users are available to import. All valid CSV
                      usernames already exist in OpenLDAP.
                    </div>
                  )}

                  <div className="space-y-2">
                    <Label htmlFor="synology-initial-password">
                      Initial password for newly imported users
                    </Label>

                    <Input
                      id="synology-initial-password"
                      type="password"
                      autoComplete="new-password"
                      value={initialPassword}
                      disabled={importing}
                      placeholder="Minimum 8 characters"
                      onChange={(event) =>
                        setInitialPassword(event.target.value)
                      }
                    />

                    <p className="text-xs text-muted-foreground">
                      Applied only to new users. Existing LDAP users
                      are not changed.
                    </p>
                  </div>

                  <div className="overflow-x-auto rounded-lg border bg-background">
                    <table className="w-full min-w-[780px] text-sm">
                      <thead className="border-b bg-muted/40">
                        <tr>
                          <th className="px-3 py-2 text-left font-medium">
                            Username
                          </th>
                          <th className="px-3 py-2 text-left font-medium">
                            Full Name
                          </th>
                          <th className="px-3 py-2 text-left font-medium">
                            Email
                          </th>
                          <th className="px-3 py-2 text-left font-medium">
                            Status
                          </th>
                          <th className="px-3 py-2 text-left font-medium">
                            Action
                          </th>
                        </tr>
                      </thead>
                      <tbody>
                        {preview.users.slice(0, 20).map((user) => (
                          <tr
                            key={`${user.rowNumber}-${user.username}`}
                            className="border-b last:border-b-0"
                          >
                            <td className="px-3 py-2 font-mono text-xs">
                              {user.username}
                            </td>
                            <td className="px-3 py-2">{user.fullName}</td>
                            <td className="px-3 py-2">
                              {user.email || "-"}
                            </td>
                            <td className="px-3 py-2">
                              <Badge
                                variant={
                                  user.enabled
                                    ? "secondary"
                                    : "outline"
                                }
                              >
                                {user.enabled ? "Enabled" : "Disabled"}
                              </Badge>
                            </td>
                            <td className="px-3 py-2">{user.action}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  {preview.users.length > 20 && (
                    <p className="text-xs text-muted-foreground">
                      Showing the first 20 of {preview.users.length} rows.
                    </p>
                  )}
                </div>
              )}
            </div>
          ) : (
            <div className="space-y-5">
              <div className="rounded-lg border border-primary/25 bg-primary/5 p-5">
                <div className="flex items-start gap-3">
                  <CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0 text-primary" />
                  <div>
                    <h3 className="font-semibold">Import Completed</h3>
                    <p className="mt-1 text-sm text-muted-foreground">
                      The Synology CSV import has finished. Review the
                      summary below before closing this dialog.
                    </p>
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3 md:grid-cols-5">
                <SummaryCard
                  label="Total rows"
                  value={result.totalRows}
                />
                <SummaryCard
                  label="Created users"
                  value={result.createdUsers}
                />
                <SummaryCard
                  label="Skipped existing"
                  value={result.skippedExistingUsers}
                />
                <SummaryCard
                  label="Disabled users"
                  value={result.disabledUsers}
                />
                <SummaryCard
                  label="Memberships added"
                  value={result.groupMembershipsAdded}
                />
              </div>

              {result.errors.length > 0 ? (
                <div className="rounded-lg border border-destructive/30 bg-destructive/5 p-4">
                  <div className="flex items-start gap-3">
                    <XCircle className="mt-0.5 h-5 w-5 shrink-0 text-destructive" />
                    <div className="min-w-0 flex-1">
                      <h3 className="font-semibold text-destructive">
                        Import completed with {result.errors.length} error(s)
                      </h3>
                      <div className="mt-3 max-h-52 overflow-y-auto rounded-md border bg-background">
                        <ul className="divide-y text-sm">
                          {result.errors.map((error, index) => (
                            <li
                              key={`${index}-${error}`}
                              className="break-words px-3 py-2"
                            >
                              {error}
                            </li>
                          ))}
                        </ul>
                      </div>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="rounded-lg border bg-muted/20 p-4 text-sm text-muted-foreground">
                  No errors were reported by the import process.
                </div>
              )}
            </div>
          )}
        </div>

        <DialogFooter className="px-6">
          {result ? (
            <>
              <Button
                type="button"
                variant="outline"
                onClick={resetState}
              >
                <RotateCcw className="mr-2 h-4 w-4" />
                Import Another File
              </Button>

              <Button
                type="button"
                onClick={() => handleOpenChange(false)}
              >
                Close
              </Button>
            </>
          ) : (
            <>
              <Button
                type="button"
                variant="outline"
                onClick={() => handleOpenChange(false)}
                disabled={uploading || importing}
              >
                Cancel
              </Button>

              {preview && (
                <Button
                  type="button"
                  disabled={importing || preview.newUsers === 0}
                  onClick={() => void handleExecuteImport()}
                >
                  {importing ? (
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  ) : (
                    <Upload className="mr-2 h-4 w-4" />
                  )}
                  Import Users
                </Button>
              )}
            </>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
