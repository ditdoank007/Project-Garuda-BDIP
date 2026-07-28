import { Badge } from "@/components/ui/badge";

interface StatusBadgeProps {
  active: boolean;
}

export default function StatusBadge({
  active,
}: StatusBadgeProps) {
  return (
    <Badge
      variant={active ? "default" : "secondary"}
      className={
        active
          ? "bg-green-600 hover:bg-green-600"
          : "bg-gray-500 hover:bg-gray-500"
      }
    >
      {active ? "Active" : "Disabled"}
    </Badge>
  );
}