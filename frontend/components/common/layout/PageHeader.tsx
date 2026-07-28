import { Button } from "@/components/ui/button";
import PageTitle from "./PageTitle";

interface PageHeaderProps {
  title: string;
  description?: string;
  count?: number;
  buttonLabel?: string;
  onAdd?: () => void;
}

export default function PageHeader({
  title,
  description,
  count,
  buttonLabel,
  onAdd,
}: PageHeaderProps) {
  return (
    <div className="flex items-center justify-between">

      <div>

        <PageTitle
          title={title}
          subtitle={description}
        />

        {count !== undefined && (
          <p className="mt-2 text-sm text-muted-foreground">
            Total : {count}
          </p>
        )}

      </div>

      {buttonLabel && (
        <Button onClick={onAdd}>
          {buttonLabel}
        </Button>
      )}

    </div>
  );
}