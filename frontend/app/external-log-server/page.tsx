import PageHeader from "@/components/common/layout/PageHeader";
import ExternalLogServerCard from "@/components/external-log-server/ExternalLogServerCard";

export default function ExternalLogServerPage() {
  return (
    <div className="space-y-6">
        <PageHeader
          title="Ext Log Server"
          description="Konfigurasi tujuan pengiriman audit log ke server syslog eksternal."
        />

        <ExternalLogServerCard />
      </div>
  );
}
