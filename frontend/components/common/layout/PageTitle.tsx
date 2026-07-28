interface PageTitleProps {
  title: string;
  subtitle?: string;
}

export default function PageTitle({
  title,
  subtitle,
}: PageTitleProps) {
  return (
    <div>
      <h1 className="text-3xl font-bold tracking-tight">
        {title}
      </h1>

      {subtitle && (
        <p className="mt-1 text-sm text-muted-foreground">
          {subtitle}
        </p>
      )}
    </div>
  );
}