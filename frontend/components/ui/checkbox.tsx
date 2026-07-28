"use client";

import * as React from "react";

interface CheckboxProps
  extends Omit<
    React.InputHTMLAttributes<HTMLInputElement>,
    "type" | "onChange"
  > {
  onCheckedChange?: (
    checked: boolean
  ) => void;
}

export function Checkbox({
  checked,
  onCheckedChange,
  className = "",
  ...props
}: CheckboxProps) {
  return (
    <input
      type="checkbox"
      checked={checked}
      className={
        "h-4 w-4 rounded border border-gray-300 " +
        className
      }
      onChange={(e) =>
        onCheckedChange?.(
          e.target.checked
        )
      }
      {...props}
    />
  );
}